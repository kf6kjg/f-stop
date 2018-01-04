// Test.cs
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

using NUnit.Framework;
using System;
using System.IO;
using RestSharp;
using System.Net;
using System.Text;
using System.Linq;

namespace f_stopHttpApiTests {
	[TestFixture]
	public class TestGetAsset {
		private Guid _capId;
		private InWorldz.Data.Assets.Stratus.StratusAsset _knownAsset;

		public static IRestResponse GetAsset(Guid capId, Guid assetId, string type = "texture_id") {
			var client = new RestClient(Constants.SERVICE_URI);
			var url = $"/CAPS/HTT/{capId.ToString("N")}?{type}={assetId.ToString("N")}";
			var request = new RestRequest(url, Method.GET);
			var response = client.Execute(request);
			return response;
		}

		[OneTimeSetUp]
		public void Setup() {
			_capId = Guid.NewGuid();
			TestAddCap.AddCap(_capId);

			_knownAsset = new InWorldz.Data.Assets.Stratus.StratusAsset {
				CreateTime = DateTime.UtcNow,
				Data = new byte[] { 0x00, 0x00, 0x00, 0x0C, 0x6A, 0x50, 0x20, 0x20, 0x0D, 0x0A, 0x87, 0x0A }, // JPEG-2000 magic numbers
				Description = "f_stopHttpApiTests KnownAsset",
				Id = Guid.NewGuid(),
				Local = true,
				Name = "KnownAsset",
				StorageFlags = 0,
				Temporary = false,
				Type = 0,
			};

			using (var file = File.Create(Path.Combine(Constants.TEST_CACHE_PATH, _knownAsset.Id.ToString("N")))) {
				ProtoBuf.Serializer.Serialize(file, _knownAsset);
			}
		}

		[Test]
		public void TestGetAssetKnownDoubleQueryBadRequest() {
			var assetIdStr = _knownAsset.Id.ToString("N");
			var client = new RestClient(Constants.SERVICE_URI);
			var url = $"/CAPS/HTT/{_capId.ToString("N")}?texture_id={assetIdStr}&mesh_id={assetIdStr}";
			var request = new RestRequest(url, Method.GET);
			var response = client.Execute(request);
			Assert.AreEqual(HttpStatusCode.BadRequest, response.StatusCode);
		}

		[Test]
		public void TestGetAssetKnownNoQueryBadRequest() {
			var client = new RestClient(Constants.SERVICE_URI);
			var url = $"/CAPS/HTT/{_capId.ToString("N")}";
			var request = new RestRequest(url, Method.GET);
			var response = client.Execute(request);
			Assert.AreEqual(HttpStatusCode.BadRequest, response.StatusCode);
		}

		[Test]
		public void TestGetAssetKnownOk() {
			var response = GetAsset(_capId, _knownAsset.Id);
			Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);
		}

		[Test]
		public void TestGetAssetKnownSame() {
			var response = GetAsset(_capId, _knownAsset.Id);
			var assetData = Encoding.ASCII.GetBytes(response.Content);
			Assert.That(assetData.SequenceEqual(_knownAsset.Data));
		}

		[Test]
		public void TestGetAssetUnknownCapNotFound() {
			var response = GetAsset(Guid.NewGuid(), _knownAsset.Id);
			Assert.AreEqual(HttpStatusCode.NotFound, response.StatusCode);
		}

		[Test]
		public void TestGetAssetUnknownAssetNotFound() {
			var response = GetAsset(_capId, Guid.NewGuid());
			Assert.AreEqual(HttpStatusCode.NotFound, response.StatusCode);
		}

	}
}
