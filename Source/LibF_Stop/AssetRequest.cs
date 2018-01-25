// AssetRequest.cs
//
// Author:
//       Ricky Curtice <ricky@rwcproductions.com>
//
// Copyright (c) 2018 Richard Curtice
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
using InWorldz.Data.Assets.Stratus;

namespace LibF_Stop {
	public class AssetRequest {
		public delegate void AssetRequestHandler(StratusAsset asset);
		public delegate void AssetErrorHandler(AssetError error);

		public Guid AssetId { get; protected set; }

		protected AssetRequestHandler _handler;
		protected AssetErrorHandler _errHandler;

		public AssetRequest(Guid assetId, AssetRequestHandler handler, AssetErrorHandler errHandler) {
			AssetId = assetId;
			_handler = handler ?? throw new ArgumentNullException(nameof(handler));
			_errHandler = errHandler ?? throw new ArgumentNullException(nameof(errHandler));
		}

		public void Respond(StratusAsset asset) {
			asset = asset ?? throw new ArgumentNullException(nameof(asset));

			if (asset.Id != AssetId) {
				throw new AssetIdMismatchException($"Expecting {AssetId}, but got asset with ID {asset.Id}");
			}

			if (_handler == null) {
				throw new AssetAlreadySetException($"Cannot call {nameof(Respond)} twice!");
			}

			_handler(asset);

			_handler = null;
			_errHandler = null;
		}

		public void Respond(AssetError err) {
			if (_handler == null) {
				throw new AssetAlreadySetException($"Cannot call {nameof(Respond)} twice!");
			}

			_errHandler(err);

			_handler = null;
			_errHandler = null;
		}
	}
}
