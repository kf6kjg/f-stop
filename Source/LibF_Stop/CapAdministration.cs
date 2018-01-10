// CapAdministration.cs
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
using System.Collections.Concurrent;

namespace LibF_Stop {
	internal class CapAdministration {
		private static readonly log4net.ILog LOG = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

		private ConcurrentDictionary<Guid, Capability> _caps = new ConcurrentDictionary<Guid, Capability>();

		// TODO: Make sure to implement a negative cache for items that are requested but are not allowed types.  Such requests should be logged with the client details so that fail2ban can catch it.

		public bool AddCap(string adminToken, Guid id, uint bandwidth = 0) {
			if (!adminToken.Equals(ConfigSingleton.AdminToken, StringComparison.OrdinalIgnoreCase)) {
				throw new InvalidAdminTokenException();
			}

			return _caps.TryAdd(id, new Capability {
				BandwidthLimit = bandwidth,
			});
		}

		public bool RemoveCap(string adminToken, Guid id) {
			if (!adminToken.Equals(ConfigSingleton.AdminToken, StringComparison.OrdinalIgnoreCase)) {
				throw new InvalidAdminTokenException();
			}

			if (_caps.TryRemove(id, out Capability cap)) {
				cap.PurgeAndKill(); // Make sure to purge the cap.

				return true;
			}

			return false;
		}

		public bool PauseCap(string adminToken, Guid id) {
			if (!adminToken.Equals(ConfigSingleton.AdminToken, StringComparison.OrdinalIgnoreCase)) {
				throw new InvalidAdminTokenException();
			}

			if (_caps.TryGetValue(id, out Capability cap)) {
				cap.Pause();

				return true;
			}

			return false;
		}

		public bool ResumeCap(string adminToken, Guid id) {
			if (!adminToken.Equals(ConfigSingleton.AdminToken, StringComparison.OrdinalIgnoreCase)) {
				throw new InvalidAdminTokenException();
			}

			if (_caps.TryGetValue(id, out Capability cap)) {
				cap.Resume();

				return true;
			}

			return false;
		}

		public bool LimitCap(string adminToken, Guid id, uint bandwidth = 0) {
			if (!adminToken.Equals(ConfigSingleton.AdminToken, StringComparison.OrdinalIgnoreCase)) {
				throw new InvalidAdminTokenException();
			}

			if (_caps.TryGetValue(id, out Capability cap)) {
				cap.BandwidthLimit = bandwidth;

				return true;
			}

			return false;
		}

		public void RequestTextureAssetOnCap(Guid capId, Guid assetId, AssetRequest.AssetRequestHandler handler, AssetRequest.AssetErrorHandler errHandler) {
			if (_caps.TryGetValue(capId, out Capability cap)) {
				cap.RequestAsset(
					assetId,
					(asset) => {
						switch (asset.Type) {
							case 0:
							case 12:
							case 18:
							case 19:
								handler(asset);
							break;
							default:
								errHandler(new AssetError {
									Error = AssetErrorType.AssetTypeWrong,
								});
							break;
						}
					},
					errHandler
				);

				return;
			}

			errHandler(new AssetError {
				Error = AssetErrorType.CapabilityIdUnknown,
			});
		}

		public void RequestMeshAssetOnCap(Guid capId, Guid assetId, AssetRequest.AssetRequestHandler handler, AssetRequest.AssetErrorHandler errHandler) {
			if (_caps.TryGetValue(capId, out Capability cap)) {
				cap.RequestAsset(
					assetId,
					(asset) => {
						if (asset.Type == 49) {
							handler(asset);
						}
						else {
							errHandler(new AssetError {
								Error = AssetErrorType.AssetTypeWrong,
							});
						}
					},
					errHandler
				);

				return;
			}

			errHandler(new AssetError {
				Error = AssetErrorType.CapabilityIdUnknown,
			});
		}
	}
}
