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
using System.Linq;

namespace LibF_Stop {
	internal class Capability {
		public const uint MAX_QUEUED_REQUESTS = 50;

		public bool IsActive { get; set; } = true; // TODO: when set true, we need to empty the queue. See handle_resume_cap

		public uint BandwidthLimit { get; set; } = 0;

		private ConcurrentQueue<AssetRequest> _requests = new ConcurrentQueue<AssetRequest>();

		public void RequestAsset(Guid assetId, AssetRequest.AssetRequestHandler handler, AssetRequest.AssetErrorHandler errHandler) {
			if (!IsActive) { // Paused
				_requests.Enqueue(new AssetRequest(assetId, handler, errHandler));

				if (_requests.Count > MAX_QUEUED_REQUESTS && _requests.TryDequeue(out AssetRequest request)) {
					request.Respond(new CapQueueFilledException());
				}

				return;
			}

			// Not paused, query Chattel.
			var reader = ConfigSingleton.ChattelReader;
			if (reader == null) {
				// There's no Chattel. Fail.
				errHandler(new ConfigException("Chattel was null!"));
				return;
			}

			var asset = reader.GetAssetSync(assetId);
			if (!ConfigSingleton.ValidTypes.Contains(asset.Type)) {
				errHandler(new WrongAssetTypeException());
				return;
			}

			handler(asset);
		}
	}
}
