﻿// Program.cs
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
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Sockets;
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

		private static readonly string DEFAULT_DB_FOLDER_PATH = "localStorage";

		private static readonly string DEFAULT_WRITECACHE_FILE_PATH = "whiplru.wcache";

		private static readonly uint DEFAULT_WRITECACHE_RECORD_COUNT = 1024U * 1024U * 1024U/*1GB*/ / 17 /*WriteCacheNode.BYTE_SIZE*/;

		private static readonly Dictionary<string, IAssetServer> _assetServersByName = new Dictionary<string, IAssetServer>();

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

				var configRead = configSource.Configs["AssetsRead"];

				var serversRead = GetServers(configSource, configRead, _assetServersByName);

				var chattelConfigRead = GetConfig(configRead, serversRead);

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

		private static IEnumerable<IEnumerable<IAssetServer>> GetServers(IConfigSource configSource, IConfig assetConfig, Dictionary<string, IAssetServer> serverList) {
			var serialParallelServerSources = assetConfig?
				.GetString("Servers", string.Empty)
				.Split(',')
				.Where(parallelSources => !string.IsNullOrWhiteSpace(parallelSources))
				.Select(parallelSources => parallelSources
					.Split('&')
					.Where(source => !string.IsNullOrWhiteSpace(source))
					.Select(source => source.Trim())
				)
				.Where(parallelSources => parallelSources.Any())
			;

			var serialParallelAssetServers = new List<List<IAssetServer>>();

			if (serialParallelServerSources != null && serialParallelServerSources.Any()) {
				foreach (var parallelSources in serialParallelServerSources) {
					var parallelServerConnectors = new List<IAssetServer>();
					foreach (var sourceName in parallelSources) {
						var sourceConfig = configSource.Configs[sourceName];
						var type = sourceConfig?.GetString("Type", string.Empty)?.ToLower(System.Globalization.CultureInfo.InvariantCulture);

						if (!serverList.TryGetValue(sourceName, out var serverConnector)) {
							try {
								switch (type) {
									case "whip":
										serverConnector = new AssetServerWHIP(
											sourceName,
											sourceConfig.GetString("Host", string.Empty),
											sourceConfig.GetInt("Port", 32700),
											sourceConfig.GetString("Password", "changeme") // Yes, that's the default password for WHIP.
										);
										break;
									case "cf":
										serverConnector = new AssetServerCF(
											sourceName,
											sourceConfig.GetString("Username", string.Empty),
											sourceConfig.GetString("APIKey", string.Empty),
											sourceConfig.GetString("DefaultRegion", string.Empty),
											sourceConfig.GetBoolean("UseInternalURL", true),
											sourceConfig.GetString("ContainerPrefix", string.Empty)
										);
										break;
									default:
										LOG.Warn($"Unknown asset server type in section [{sourceName}].");
										break;
								}

								serverList.Add(sourceName, serverConnector);
							}
							catch (SocketException e) {
								LOG.Error($"Asset server of type '{type}' defined in section [{sourceName}] failed setup. Skipping server.", e);
							}
						}

						if (serverConnector != null) {
							parallelServerConnectors.Add(serverConnector);
						}
					}

					if (parallelServerConnectors.Any()) {
						serialParallelAssetServers.Add(parallelServerConnectors);
					}
				}
			}
			else {
				LOG.Warn("Servers empty or not specified. No asset server sections configured.");
			}

			return serialParallelAssetServers;
		}

		private static ChattelConfiguration GetConfig(IConfig assetConfig, IEnumerable<IEnumerable<IAssetServer>> serialParallelAssetServers) {
			// Set up local storage
			var localStoragePathRead = assetConfig?.GetString("DatabaseFolderPath", DEFAULT_DB_FOLDER_PATH) ?? DEFAULT_DB_FOLDER_PATH;

			DirectoryInfo localStorageFolder = null;

			if (string.IsNullOrWhiteSpace(localStoragePathRead)) {
				LOG.Info($"DatabaseFolderPath is empty, local storage of assets disabled.");
			}
			else if (!Directory.Exists(localStoragePathRead)) {
				LOG.Info($"DatabaseFolderPath folder does not exist, local storage of assets disabled.");
			}
			else {
				localStorageFolder = new DirectoryInfo(localStoragePathRead);
				LOG.Info($"Local storage of assets enabled at {localStorageFolder.FullName}");
			}

			// Set up write cache
			var writeCachePath = assetConfig?.GetString("WriteCacheFilePath", DEFAULT_WRITECACHE_FILE_PATH) ?? DEFAULT_WRITECACHE_FILE_PATH;
			var writeCacheRecordCount = (uint)Math.Max(0, assetConfig?.GetLong("WriteCacheRecordCount", DEFAULT_WRITECACHE_RECORD_COUNT) ?? DEFAULT_WRITECACHE_RECORD_COUNT);

			if (string.IsNullOrWhiteSpace(writeCachePath) || writeCacheRecordCount <= 0 || localStorageFolder == null) {
				LOG.Warn($"WriteCacheFilePath is empty, WriteCacheRecordCount is zero, or caching is disabled. Crash recovery will be compromised.");
			}
			else {
				var writeCacheFile = new FileInfo(writeCachePath);
				LOG.Info($"Write cache enabled at {writeCacheFile.FullName} with {writeCacheRecordCount} records.");
			}

			return new ChattelConfiguration(localStoragePathRead, writeCachePath, writeCacheRecordCount, serialParallelAssetServers);
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
