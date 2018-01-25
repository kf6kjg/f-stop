// Capability.cs
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
using System.Collections.Generic;
using System.Linq;
using System.Threading;

namespace LibF_Stop {
	internal class Capability {
		public const uint MAX_QUEUED_REQUESTS = 50;

		public bool IsActive { get; protected set; } = true;

		public uint BandwidthLimit { get; set; } = 0;

		private ConcurrentQueue<AssetRequest> _requests = new ConcurrentQueue<AssetRequest>();

		public void RequestAsset(Guid assetId, AssetRequest.AssetRequestHandler handler, AssetRequest.AssetErrorHandler errHandler) {
			if (!IsActive) { // Paused
				AssetRequest requestToProcess = null;
				var gotLock = false;
				try { // Skiplock: Only locking because of possible shutdown action.
					Monitor.TryEnter(_requests, ref gotLock);

					if (gotLock) {
						_requests.Enqueue(new AssetRequest(assetId, handler, errHandler));

						if (_requests.Count > MAX_QUEUED_REQUESTS) {
							_requests.TryDequeue(out requestToProcess);
						}
					}
				}
				finally {
					if (gotLock) {
						Monitor.Exit(_requests);
					}
				}

				var errors = new Queue<Exception>(); // Have to make sure both operations below happen, even if they throw.

				if (!gotLock) {
					// It seems that this cap cannot queue the request. Probably shutting down. Tell the client no such luck.
					try {
						errHandler(new AssetError {
							Error = AssetErrorType.QueueFilled,
						});
					}
					catch (Exception e) {
						errors.Enqueue(e);
					}
				}

				try {
					requestToProcess?.Respond(new AssetError {
						Error = AssetErrorType.QueueFilled,
					});
				}
				catch (Exception e) {
					errors.Enqueue(e);
				}

				if (errors.Count > 0) {
					throw new AggregateException(errors);
				}

				return;
			}

			// Not paused, query Chattel.
			var reader = ConfigSingleton.ChattelReader;
			if (reader == null) {
				// There's no Chattel. Fail.
				errHandler(new AssetError {
					Error = AssetErrorType.ConfigIncorrect,
					Message = "Chattel was null!",
				});
				return;
			}

			reader.GetAssetAsync(assetId, asset => {
				if (asset == null) {
					errHandler(new AssetError {
						Error = AssetErrorType.AssetIdUnknown,
						Message = $"Could not find any asset with ID {assetId}",
					});
					return;
				}

				if (!ConfigSingleton.ValidTypes.Contains(asset.Type)) {
					errHandler(new AssetError {
						Error = AssetErrorType.AssetTypeWrong,
					});
					return;
				}

				handler(asset);
			});
		}

		public void Pause() {
			IsActive = false;
		}

		public void PurgeAndKill() {
			lock (_requests) { // Lock to prevent the addition of more requests to this cap, and to wait on any being added.
				IsActive = false;
				PurgeQueue();
			}
		}

		public void Resume() {
			PurgeQueue();
			IsActive = true;
		}


		private void PurgeQueue() {
			// TODO fullfil all the queued asset requests
		}
	}
}
