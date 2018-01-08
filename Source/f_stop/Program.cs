// Program.cs
//
// Author:
//       Ricky Curtice <ricky@rwcproductions.com>
//
// Copyright (c) 2017 Richard Curtice
//
// Permission is hereby granted, free of charge, to any person obtaining a copy
// of this software and associated documentation files (the "Software"), to deal
// in the Software without restriction, including without limitation the rights
// to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
// copies of the Software, and to permit persons to whom the Software is
// furnished to do so, subject to the following conditions:
//
// The above copyright notice and this permission notice shall be included in
// all copies or substantial portions of the Software.
//
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
// OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
// THE SOFTWARE.

using System;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading;
using Chattel;
using LibF_Stop;
using log4net;
using log4net.Config;
using Nini.Config;

namespace f_stop {
	class MainClass {
		private static readonly ILog LOG = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

		private static readonly bool ON_POSIX_COMPLAINT_OS = Type.GetType("Mono.Runtime") != null; // A potentially invalid assumption: that Mono means running on a POSIX-compliant system.

		private static readonly string EXECUTABLE_DIRECTORY = Path.GetDirectoryName(Assembly.GetEntryAssembly().CodeBase.Replace(ON_POSIX_COMPLAINT_OS ? "file:/" : "file:///", string.Empty));

		private static readonly string DEFAULT_INI_FILE = Path.Combine(EXECUTABLE_DIRECTORY, "f_stop.ini");

		private static readonly string COMPILED_BY = "?mono?"; // Replaced during automatic packaging.

		public static int Main(string[] args) {
			// First line, hook the appdomain to the crash reporter
			AppDomain.CurrentDomain.UnhandledException += CurrentDomain_UnhandledException;

			var createdNew = true;
			var waitHandle = new EventWaitHandle(false, EventResetMode.AutoReset, "70a9f94f-59e8-4073-93ab-00aaacc26111", out createdNew);

			if (!createdNew) {
				LOG.Error("Server process already started, please stop that server first.");
				return 2;
			}

			// Add the arguments supplied when running the application to the configuration
			var configSource = new ArgvConfigSource(args);

			// Commandline switches
			configSource.AddSwitch("Startup", "inifile");
			configSource.AddSwitch("Startup", "logconfig");
			configSource.AddSwitch("Startup", "pidfile");

			var startupConfig = configSource.Configs["Startup"];

			var pidFileManager = new PIDFileManager(startupConfig.GetString("pidfile", string.Empty));

			// Configure Log4Net
			var logConfigFile = startupConfig.GetString("logconfig", string.Empty);
			if (string.IsNullOrEmpty(logConfigFile)) {
				XmlConfigurator.Configure();
				LogBootMessage();
				LOG.Info("Configured log4net using ./WHIP_LRU.exe.config as the default.");
			}
			else {
				XmlConfigurator.Configure(new FileInfo(logConfigFile));
				LogBootMessage();
				LOG.Info($"Configured log4net using \"{logConfigFile}\" as configuration file.");
			}

			// Configure nIni aliases and locale
			Thread.CurrentThread.CurrentCulture = new System.Globalization.CultureInfo("en-US", true);

			configSource.Alias.AddAlias("On", true);
			configSource.Alias.AddAlias("Off", false);
			configSource.Alias.AddAlias("True", true);
			configSource.Alias.AddAlias("False", false);
			configSource.Alias.AddAlias("Yes", true);
			configSource.Alias.AddAlias("No", false);

			var isRunning = true;

			F_Stop f_stop = null;

			// Handlers for signals.
			Console.CancelKeyPress += (sender, cargs) => {
				LOG.Debug("CTRL-C pressed, terminating.");
				isRunning = false;
				f_stop?.Stop();

				cargs.Cancel = true;
				waitHandle.Set();
			};

			// TODO: incorporate UNIX signals for reloading etc, once the crossplatform kinks have been worked out in WHIP-LRU.

			while (isRunning) {
				// Read in the ini file
				ReadConfigurationFromINI(configSource);

				var chattelConfigRead = new ChattelConfiguration(configSource, configSource.Configs["AssetsRead"]);

				var chattelReader = new ChattelReader(chattelConfigRead);

				var serverConfig = configSource.Configs["Server"];

				var address = serverConfig?.GetString("Address", F_Stop.DEFAULT_ADDRESS) ?? F_Stop.DEFAULT_ADDRESS;
				if (address == "*") {
					address = "localhost";
				}
				var port = (uint?)serverConfig?.GetInt("Port", (int)F_Stop.DEFAULT_PORT) ?? F_Stop.DEFAULT_PORT;
				var useSSL = serverConfig?.GetBoolean("UseSSL", F_Stop.DEFAULT_USE_SSL) ?? F_Stop.DEFAULT_USE_SSL;
				var adminToken = serverConfig?.GetString("AdminToken", F_Stop.DEFAULT_ADMIN_TOKEN) ?? F_Stop.DEFAULT_ADMIN_TOKEN;
				var validAssetTypes = serverConfig?.GetString("AllowedAssetTypes", F_Stop.DEFAULT_VALID_ASSET_TYPES) ?? F_Stop.DEFAULT_VALID_ASSET_TYPES;

				var cacheConfig = configSource.Configs["Cache"];

				var negativeCacheItemLifetime = TimeSpan.FromSeconds((uint?)cacheConfig?.GetInt("NegativeCacheItemLifetimeSeconds", (int)F_Stop.DEFAULT_NC_LIFETIME_SECONDS) ?? F_Stop.DEFAULT_NC_LIFETIME_SECONDS);

				var protocol = useSSL ? "https" : "http";

				var uri = new Uri($"{protocol}://{address}:{port}");
				f_stop = new F_Stop(
					uri,
					adminToken,
					negativeCacheItemLifetime,
					chattelReader,
					validAssetTypes.Split(',').Select(type => sbyte.Parse(type))
				);

				f_stop.Start();

				waitHandle.WaitOne();
			}

			return 0;
		}

		#region Bootup utils

		private static void LogBootMessage() {
			LOG.Info("* * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * *");
			LOG.Info($"f_stop v{Assembly.GetExecutingAssembly().GetName().Version.ToString()} {COMPILED_BY}");
			var bitdepth = Environment.Is64BitOperatingSystem ? "64bit" : "unknown or 32bit";
			LOG.Info($"OS: {Environment.OSVersion.VersionString} {bitdepth}");
			LOG.Info($"Commandline: {Environment.CommandLine}");
			LOG.Info($"CWD: {Environment.CurrentDirectory}");
			LOG.Info($"Machine: {Environment.MachineName}");
			LOG.Info($"Processors: {Environment.ProcessorCount}");
			LOG.Info($"User: {Environment.UserDomainName}/{Environment.UserName}");
			var isMono = Type.GetType("Mono.Runtime") != null;
			LOG.Info("Interactive shell: " + (Environment.UserInteractive ? "yes" : isMono ? "indeterminate" : "no"));
			LOG.Info("* * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * *");
		}

		private static void ReadConfigurationFromINI(IConfigSource configSource) {
			var startupConfig = configSource.Configs["Startup"];
			var iniFileName = startupConfig.GetString("inifile", DEFAULT_INI_FILE);

			var found_at_given_path = false;

			try {
				LOG.Info($"Attempting to read configuration file {Path.GetFullPath(iniFileName)}");
				startupConfig.ConfigSource.Merge(new IniConfigSource(iniFileName));
				LOG.Info($"Success reading configuration file.");
				found_at_given_path = true;
			}
			catch {
				LOG.Warn($"Failure reading configuration file at {Path.GetFullPath(iniFileName)}");
			}

			if (!found_at_given_path) {
				// Combine with true path to binary and try again.
				iniFileName = Path.Combine(EXECUTABLE_DIRECTORY, iniFileName);

				try {
					LOG.Info($"Attempting to read configuration file from installation path {Path.GetFullPath(iniFileName)}");
					startupConfig.ConfigSource.Merge(new IniConfigSource(iniFileName));
					LOG.Info($"Success reading configuration file.");
				}
				catch {
					LOG.Fatal($"Failure reading configuration file at {Path.GetFullPath(iniFileName)}");
				}
			}
		}

		#endregion

		#region Crash handler

		private static bool _isHandlingException;

		/// <summary>
		/// Global exception handler -- all unhandled exceptions end up here :)
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private static void CurrentDomain_UnhandledException(object sender, UnhandledExceptionEventArgs e) {
			if (_isHandlingException) {
				return;
			}

			try {
				_isHandlingException = true;

				var msg = string.Empty;

				var ex = (Exception)e.ExceptionObject;
				if (ex.InnerException != null) {
					msg = $"InnerException: {ex.InnerException}\n";
				}

				msg = $"APPLICATION EXCEPTION DETECTED: {e}\n" +
					"\n" +
					$"Exception: {e.ExceptionObject}\n" +
					msg +
					$"\nApplication is terminating: {e.IsTerminating}\n";

				LOG.Fatal(msg);

				if (e.IsTerminating) {
					// Since we are crashing, there's no way that log4net.RollbarNET will be able to send the message to Rollbar directly.
					// So have a separate program go do that work while this one finishes dying.

					// TODO: At some point re-integrate the Rollbar crash reporter.
				}
			}
			catch (Exception ex) {
				LOG.Error("Exception launching CrashReporter.", ex);
			}
			finally {
				_isHandlingException = false;

				if (e.IsTerminating) {
					// Preempt to not show a pile of puke if console was disabled.
					Environment.Exit(1);
				}
			}
		}

		#endregion
	}
}
