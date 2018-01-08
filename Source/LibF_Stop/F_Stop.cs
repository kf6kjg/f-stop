// F_Stop.cs
//
// Author:
//       ricky <>
//
// Copyright (c) 2017 ${CopyrightHolder}
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
using Nancy.Hosting.Self;

namespace LibF_Stop {
	public sealed class F_Stop : IDisposable {
		public const string DEFAULT_ADDRESS = "*";
		public const uint DEFAULT_PORT = 8000;
		public const bool DEFAULT_USE_SSL = false;
		public const string DEFAULT_ADMIN_TOKEN = "changemenow";
		public const uint DEFAULT_NC_LIFETIME_SECONDS = 120;
		public const string DEFAULT_VALID_ASSET_TYPES = "0,49";

		private readonly NancyHost _host;

		public F_Stop(Uri uri, string adminToken, TimeSpan negativeCacheItemLifetime, Chattel.ChattelReader chattelReader, System.Collections.Generic.IEnumerable<sbyte> validTypes) {
			adminToken = adminToken ?? throw new ArgumentNullException(nameof(adminToken));
			if (negativeCacheItemLifetime.Ticks <= 0) {
				throw new ArgumentOutOfRangeException(nameof(negativeCacheItemLifetime), "NegativeCacheItemLifetime cannot be 0 or negative.");
			}

			ConfigSingleton.AdminToken = adminToken;
			ConfigSingleton.NegativeCacheItemLifetime = negativeCacheItemLifetime;
			ConfigSingleton.ChattelReader = chattelReader;
			ConfigSingleton.ValidTypes = new System.Collections.Concurrent.ConcurrentBag<sbyte>(validTypes);

			// See https://github.com/NancyFx/Nancy/wiki/Self-Hosting-Nancy#namespace-reservations
			var hostConfigs = new HostConfiguration();
			hostConfigs.UrlReservations.CreateAutomatically = true;

			_host = new NancyHost(hostConfigs, uri);
		}

		public void Start() {
			if (!disposedValue) {
				_host.Start();
			}
		}

		public void Stop() {
			if (!disposedValue) {
				_host.Stop();
			}
		}

		#region IDisposable Support

		private bool disposedValue; // To detect redundant calls

		private void Dispose(bool disposing) {
			if (!disposedValue) {
				if (disposing) {
					_host.Dispose();
				}

				disposedValue = true;
			}
		}

		// This code added to correctly implement the disposable pattern.
		public void Dispose() {
			// Do not change this code. Put cleanup code in Dispose(bool disposing) above.
			Dispose(true);
		}

		#endregion
	}
}
