// TestAssetRequest.cs
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
using System.IO;
using InWorldz.Data.Assets.Stratus;
using LibF_Stop;
using NUnit.Framework;

namespace f_stopUnitTests {
	[TestFixture]
	public class TestAssetRequest {
		private StratusAsset _asset = new StratusAsset {
			Id = Guid.NewGuid(),
		};

		#region Ctor

		[Test]
		public void TestAssetRequest_Ctor_BlankHandlers_DoesNotThrow() {
			Assert.DoesNotThrow(() => new AssetRequest(Guid.NewGuid(), a => { }, a => { }));
		}

		[Test]
		public void TestAssetRequest_Ctor_Handler1Null_ArgumentNullException() {
			Assert.Throws<ArgumentNullException>(() => new AssetRequest(Guid.NewGuid(), null, a => { }));
		}

		[Test]
		public void TestAssetRequest_Ctor_Handler2Null_ArgumentNullException() {
			Assert.Throws<ArgumentNullException>(() => new AssetRequest(Guid.NewGuid(), a => { }, null));
		}

		[Test]
		public void TestAssetRequest_Ctor_SetsAssetId() {
			var guid = Guid.NewGuid();
			var req = new AssetRequest(guid, a => { }, a => { });
			Assert.AreEqual(guid, req.AssetId);
		}

		#endregion

		#region Respond

		[Test]
		public void TestAssetRequest_Respond_StratusAsset_null_ArgumentNullException() {
			var req = new AssetRequest(Guid.NewGuid(), a => { }, a => { });
			Assert.Throws<ArgumentNullException>(() => req.Respond(null));
		}

		[Test]
		public void TestAssetRequest_Respond_StratusAsset_WrongId_AssetIdMismatchException() {
			var req = new AssetRequest(Guid.NewGuid(), a => { }, a => { });
			Assert.Throws<AssetIdMismatchException>(() => req.Respond(_asset));
		}

		[Test]
		public void TestAssetRequest_Respond_StratusAsset_DoesNotThrow() {
			var req = new AssetRequest(_asset.Id, a => { }, a => { });
			Assert.DoesNotThrow(() => req.Respond(_asset));
		}

		[Test]
		public void TestAssetRequest_Respond_StratusAsset_StratusAsset_AssetAlreadySetException() {
			var req = new AssetRequest(_asset.Id, a => { }, a => { });
			req.Respond(_asset);
			Assert.Throws<AssetAlreadySetException>(() => req.Respond(_asset));
		}

		[Test]
		public void TestAssetRequest_Respond_StratusAsset_AssetError_AssetAlreadySetException() {
			var req = new AssetRequest(_asset.Id, a => { }, a => { });
			req.Respond(_asset);
			Assert.Throws<AssetAlreadySetException>(() => req.Respond(new AssetError()));
		}

		[Test]
		public void TestAssetRequest_Respond_StratusAsset_CallsHandler() {
			var gotCallback = false;
			var req = new AssetRequest(_asset.Id, a => gotCallback = true, a => { });
			req.Respond(_asset);
			Assert.True(gotCallback);
		}


		[Test]
		public void TestAssetRequest_Respond_AssetError_DoesNotThrow() {
			var req = new AssetRequest(_asset.Id, a => { }, a => { });
			Assert.DoesNotThrow(() => req.Respond(_asset));
		}


		[Test]
		public void TestAssetRequest_Respond_AssetError_AssetError_AssetAlreadySetException() {
			var req = new AssetRequest(_asset.Id, a => { }, a => { });
			req.Respond(new AssetError());
			Assert.Throws<AssetAlreadySetException>(() => req.Respond(new AssetError()));
		}

		[Test]
		public void TestAssetRequest_Respond_AssetError_StratusAsset_AssetAlreadySetException() {
			var req = new AssetRequest(_asset.Id, a => { }, a => { });
			req.Respond(_asset);
			Assert.Throws<AssetAlreadySetException>(() => req.Respond(_asset));
		}

		[Test]
		public void TestAssetRequest_Respond_AssetError_CallsHandler() {
			var gotCallback = false;
			var req = new AssetRequest(_asset.Id, a => { }, a => gotCallback = true);
			req.Respond(new AssetError());
			Assert.True(gotCallback);
		}

		#endregion
	}
}
