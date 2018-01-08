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
using InWorldz.Data.Assets.Stratus;

namespace f_stopHttpApiTests {
	[TestFixture]
	public class TestGetAsset {
		private Guid _capId;
		private StratusAsset _knownTextureAsset;
		private StratusAsset _knownTextureTGAAsset;
		private StratusAsset _knownImageTGAAsset;
		private StratusAsset _knownImageJPEGAsset;
		private StratusAsset _knownMeshAsset;
		private StratusAsset _knownNotecardAsset;

		public static IRestResponse GetAsset(Guid capId, Guid assetId, string type = "texture_id") {
			var client = new RestClient(Constants.SERVICE_URI);
			var url = $"/CAPS/HTT/{capId.ToString("N")}?{type}={assetId.ToString("N")}";
			var request = new RestRequest(url, Method.GET);
			var response = client.Execute(request);
			return response;
		}

		public static StratusAsset CreateAndCacheAsset(string name, sbyte type, byte[] data, Guid? id = null) {
			var asset = new StratusAsset {
				CreateTime = DateTime.UtcNow,
				Data = data,
				Description = $"{name} description",
				Id = id ?? Guid.NewGuid(),
				Local = true,
				Name = name,
				StorageFlags = 0,
				Temporary = false,
				Type = type,
			};

			var assetPath = UuidToCachePath(asset.Id);

			Directory.CreateDirectory(Directory.GetParent(assetPath).FullName);
			using (var file = File.Create(assetPath)) {
				ProtoBuf.Serializer.Serialize(file, asset);
			}

			return asset;
		}

		/// <summary>
		/// Converts a GUID to a path based on the cache location.
		/// </summary>
		/// <returns>The path.</returns>
		/// <param name="id">Asset identifier.</param>
		private static string UuidToCachePath(Guid id) {
			var noPunctuationAssetId = id.ToString("N");
			var path = Constants.TEST_CACHE_PATH;
			for (var index = 0; index < noPunctuationAssetId.Length; index += 2) {
				path = Path.Combine(path, noPunctuationAssetId.Substring(index, 2));
			}
			return path + ".pbasset";
		}

		[OneTimeSetUp]
		public void Setup() {
			_capId = Guid.NewGuid();
			TestAddCap.AddCap(_capId);

			// Using hardcoded GUIDs to make debugging easier.

			_knownTextureAsset = CreateAndCacheAsset(
				"_knownTextureAsset",
				0,
				new byte[] { 0x00, 0x00, 0x00, 0x0C, 0x6A, 0x50, 0x20, 0x20, 0x0D, 0x0A, 0x87, 0x0A }, // JPEG-2000 magic numbers
				Guid.Parse("01000000-0000-0000-0000-000000000000")
			);

			_knownTextureTGAAsset = CreateAndCacheAsset(
				"_knownTextureTGAAsset",
				12,
				new byte[] { 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0x54, 0x52, 0x55, 0x45, 0x56, 0x49, 0x53, 0x49, 0x4f, 0x4e, 0x2d, 0x58, 0x46, 0x49, 0x4c, 0x45, 0x2e, 0x00 }, // TGA uses a footer for some silly historical/legacy reason.  I made some educated guesses for making this a minimal valid TGA, but i've not verified.
				Guid.Parse("02000000-0000-0000-0000-000000000000")
			);

			_knownImageTGAAsset = CreateAndCacheAsset(
				"_knownImageTGAAsset",
				18,
				new byte[] { 1, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0x54, 0x52, 0x55, 0x45, 0x56, 0x49, 0x53, 0x49, 0x4f, 0x4e, 0x2d, 0x58, 0x46, 0x49, 0x4c, 0x45, 0x2e, 0x00 }, // TGA uses a footer for some silly historical/legacy reason.  I made some educated guesses for making this a minimal valid TGA, but i've not verified.
				Guid.Parse("03000000-0000-0000-0000-000000000000")
			);

			_knownImageJPEGAsset = CreateAndCacheAsset(
				"_knownImageJPEGAsset",
				19,
				new byte[] { 0xFF, 0xD8, 0xFF, 0xE1, 0x00, 0x00, 0x45, 0x78, 0x69, 0x66, 0x00 }, // JPEG-EXIF magic numbers, guessing that's what is used but I could totally be wrong.
				Guid.Parse("04000000-0000-0000-0000-000000000000")
			);

			_knownMeshAsset = CreateAndCacheAsset(
				"_knownMeshAsset",
				49,
				new byte[] { 0xfa }, // TODO: find out what this one's is if any.
				Guid.Parse("05000000-0000-0000-0000-000000000000")
			);

			_knownNotecardAsset = CreateAndCacheAsset(
				"_knownNotecardAsset",
				7,
				Encoding.UTF8.GetBytes("Just some text."),
				Guid.Parse("06000000-0000-0000-0000-000000000000")
			);
		}

		[Test]
		public void TestGetAssetKnownDoubleQueryBadRequest() {
			var assetIdStr = _knownTextureAsset.Id.ToString("N");
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
		public void TestGetAssetKnownImageJPEGBadRequest() {
			var response = GetAsset(_capId, _knownImageJPEGAsset.Id);
			Assert.AreEqual(HttpStatusCode.BadRequest, response.StatusCode);
		}

		[Test]
		public void TestGetAssetKnownImageTGABadRequest() {
			var response = GetAsset(_capId, _knownImageTGAAsset.Id);
			Assert.AreEqual(HttpStatusCode.BadRequest, response.StatusCode);
		}

		[Test]
		public void TestGetAssetKnownMeshContentType() {
			var response = GetAsset(_capId, _knownMeshAsset.Id, "mesh_id");
			Assert.AreEqual("application/vnd.ll.mesh", response.ContentType);
		}

		[Test]
		public void TestGetAssetKnownMeshOk() {
			var response = GetAsset(_capId, _knownMeshAsset.Id, "mesh_id");
			Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);
		}

		[Test]
		public void TestGetAssetKnownTextureContentType() {
			var response = GetAsset(_capId, _knownTextureAsset.Id);
			Assert.AreEqual("image/x-j2c", response.ContentType);
		}

		[Test]
		public void TestGetAssetKnownTextureOk() {
			var response = GetAsset(_capId, _knownTextureAsset.Id);
			Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);
		}

		[Test]
		public void TestGetAssetKnownTextureSame() {
			var response = GetAsset(_capId, _knownTextureAsset.Id);
			var assetData = response.RawBytes;
			Assert.That(assetData.SequenceEqual(_knownTextureAsset.Data));
		}

		[Test]
		public void TestGetAssetKnownTextureTGAContentType() {
			var response = GetAsset(_capId, _knownTextureTGAAsset.Id);
			Assert.AreEqual("image/x-tga", response.ContentType); // That MIME type is an extension to the 
		}

		[Test]
		public void TestGetAssetKnownTextureTGAOk() {
			var response = GetAsset(_capId, _knownTextureTGAAsset.Id);
			Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);
		}

		[Test]
		public void TestGetAssetKnownTextureTGASame() {
			var response = GetAsset(_capId, _knownTextureTGAAsset.Id);
			var assetData = response.RawBytes;
			Assert.That(assetData.SequenceEqual(_knownTextureTGAAsset.Data));
		}

		[Test]
		public void TestGetAssetUnknownCapNotFound() {
			var response = GetAsset(Guid.NewGuid(), _knownTextureAsset.Id);
			Assert.AreEqual(HttpStatusCode.NotFound, response.StatusCode);
		}

		[Test]
		public void TestGetAssetUnknownAssetNotFound() {
			var response = GetAsset(_capId, Guid.NewGuid());
			Assert.AreEqual(HttpStatusCode.NotFound, response.StatusCode);
		}

	}
}
