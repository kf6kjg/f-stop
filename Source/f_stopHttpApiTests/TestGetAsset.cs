// TestGetAsset.cs
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
using System.Net;
using System.Text;
using System.Linq;
using InWorldz.Data.Assets.Stratus;
using LibF_Stop;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;

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

		public static void ForceHeader(HttpWebRequest request, string name, string value) {
			request.Headers.GetType().InvokeMember( // Use Reflection to force the header through, bypassing all of MS's sillyness about blocking invalid ranges.
				"ChangeInternal",
				System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.InvokeMethod,
				null,
				request.Headers,
				new object[] { name, value }
			);
		}

		public static HttpWebResponse GetAsset(Guid capId, Guid assetId, string type = "texture_id", IEnumerable<Range> ranges = null, TimeSpan? timeout = null) {
			var url = $"{Constants.SERVICE_URI}/CAPS/HTT/{capId.ToString("N")}?{type}={assetId.ToString("N")}";
			var request = WebRequest.Create(new Uri(Constants.SERVICE_URI, url)) as HttpWebRequest;
			request.Method = "GET";
			if (ranges != null) {
				// Aperture only supports single range, but this supports much more.
				var rangesFormatted = ranges
					.Select(range => range.ToString())
					.Where(range => range != null)
					.Aggregate((aggregate, newRange) => $"{aggregate},{newRange}")
				;
				ForceHeader(request, "Range", $"bytes={rangesFormatted}");
			}

			if (timeout != null) {
				request.Timeout = (int)timeout?.TotalMilliseconds;
			}

			return request.GetResponse() as HttpWebResponse;
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

		[OneTimeTearDown]
		public void CleanupCache() {
			try {
				Directory.Delete(Constants.TEST_CACHE_PATH, true);
			}
			catch (DirectoryNotFoundException) {
			}
		}

		[OneTimeSetUp]
		public void Setup() {
			CleanupCache();

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
				new byte[] { 0xfa }, // TODO: find out what mesh's header is if any.
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
			var url = $"/CAPS/HTT/{_capId.ToString("N")}?texture_id={assetIdStr}&mesh_id={assetIdStr}";
			var request = WebRequest.Create(new Uri(Constants.SERVICE_URI, url)) as HttpWebRequest;
			request.Method = "GET";
			try {
				var response = (HttpWebResponse)request.GetResponse();
				Assert.AreEqual(HttpStatusCode.BadRequest, response.StatusCode);
			}
			catch (WebException e) {
				Assert.AreEqual(HttpStatusCode.BadRequest, ((HttpWebResponse)e.Response).StatusCode);
			}
		}

		[Test]
		public void TestGetAssetKnownNoQueryBadRequest() {
			var client = new HttpClient();
			var url = $"/CAPS/HTT/{_capId.ToString("N")}";
			var request = WebRequest.Create(new Uri(Constants.SERVICE_URI, url)) as HttpWebRequest;
			request.Method = "GET";
			try {
				var response = (HttpWebResponse)request.GetResponse();
				Assert.AreEqual(HttpStatusCode.BadRequest, response.StatusCode);
			}
			catch (WebException e) {
				Assert.AreEqual(HttpStatusCode.BadRequest, ((HttpWebResponse)e.Response).StatusCode);
			}
		}

		[Test]
		public void TestGetAssetKnownImageJPEGBadRequest() {
			try {
				var response = GetAsset(_capId, _knownImageJPEGAsset.Id);
				Assert.AreEqual(HttpStatusCode.BadRequest, response.StatusCode);
			}
			catch (WebException e) {
				Assert.AreEqual(HttpStatusCode.BadRequest, ((HttpWebResponse)e.Response).StatusCode);
			}
		}

		[Test]
		public void TestGetAssetKnownImageTGABadRequest() {
			try {
				var response = GetAsset(_capId, _knownImageTGAAsset.Id);
				Assert.AreEqual(HttpStatusCode.BadRequest, response.StatusCode);
			}
			catch (WebException e) {
				Assert.AreEqual(HttpStatusCode.BadRequest, ((HttpWebResponse)e.Response).StatusCode);
			}
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
		public async Task TestGetAssetKnownTextureSame() {
			var response = GetAsset(_capId, _knownTextureAsset.Id);
			var assetData = new byte[_knownTextureAsset.Data.Length];
			var bytesRead = await response.GetResponseStream().ReadAsync(assetData, 0, assetData.Length);
			Assert.That(assetData.SequenceEqual(_knownTextureAsset.Data));
		}

		[Test]
		public void TestGetAssetKnownTextureTGAContentType() {
			var response = GetAsset(_capId, _knownTextureTGAAsset.Id);
			Assert.AreEqual("image/x-tga", response.ContentType); // That MIME type is an extension to the official spec.
		}

		[Test]
		public void TestGetAssetKnownTextureTGAOk() {
			var response = GetAsset(_capId, _knownTextureTGAAsset.Id);
			Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);
		}

		[Test]
		public async Task TestGetAssetKnownTextureTGASame() {
			var response = GetAsset(_capId, _knownTextureTGAAsset.Id);
			var assetData = new byte[_knownTextureTGAAsset.Data.Length];
			var bytesRead = await response.GetResponseStream().ReadAsync(assetData, 0, assetData.Length);
			Assert.That(assetData.SequenceEqual(_knownTextureTGAAsset.Data));
		}

		#region Range errors

		[Test]
		public void TestGetAssetKnownTextureByteRange_0_RequestedRangeNotSatisfiable() {
			// Aperture has this block commented out, resulting in the error to match spec.
			var url = $"/CAPS/HTT/{_capId.ToString("N")}?texture_id={_knownTextureTGAAsset.Id.ToString("N")}";
			var request = WebRequest.Create(new Uri(Constants.SERVICE_URI, url)) as HttpWebRequest;
			request.Method = "GET";
			ForceHeader(request, "Range", "bytes=0");
			try {
				var response = (HttpWebResponse)request.GetResponse();
				Assert.AreEqual(HttpStatusCode.RequestedRangeNotSatisfiable, response.StatusCode);
			}
			catch (WebException e) {
				Assert.AreEqual(HttpStatusCode.RequestedRangeNotSatisfiable, ((HttpWebResponse)e.Response).StatusCode);
			}
		}

		[Test]
		public void TestGetAssetKnownTextureByteRange_9999dash_RequestedRangeNotSatisfiable() {
			var url = $"/CAPS/HTT/{_capId.ToString("N")}?texture_id={_knownTextureTGAAsset.Id.ToString("N")}";
			var request = WebRequest.Create(new Uri(Constants.SERVICE_URI, url)) as HttpWebRequest;
			request.Method = "GET";
			ForceHeader(request, "Range", $"bytes=9999-");
			try {
				var response = (HttpWebResponse)request.GetResponse();
				Assert.AreEqual(HttpStatusCode.RequestedRangeNotSatisfiable, response.StatusCode);
			}
			catch (WebException e) {
				Assert.AreEqual(HttpStatusCode.RequestedRangeNotSatisfiable, ((HttpWebResponse)e.Response).StatusCode);
			}
		}

		[Test]
		public void TestGetAssetKnownTextureByteRange_9999dash99999_RequestedRangeNotSatisfiable() {
			var url = $"/CAPS/HTT/{_capId.ToString("N")}?texture_id={_knownTextureTGAAsset.Id.ToString("N")}";
			var request = WebRequest.Create(new Uri(Constants.SERVICE_URI, url)) as HttpWebRequest;
			request.Method = "GET";
			ForceHeader(request, "Range", $"bytes=9999-9999");
			try {
				var response = (HttpWebResponse)request.GetResponse();
				Assert.AreEqual(HttpStatusCode.RequestedRangeNotSatisfiable, response.StatusCode);
			}
			catch (WebException e) {
				Assert.AreEqual(HttpStatusCode.RequestedRangeNotSatisfiable, ((HttpWebResponse)e.Response).StatusCode);
			}
		}

		[Test]
		public void TestGetAssetKnownTextureByteRange_asdf_RequestedRangeNotSatisfiable() {
			var url = $"/CAPS/HTT/{_capId.ToString("N")}?texture_id={_knownTextureTGAAsset.Id.ToString("N")}";
			var request = WebRequest.Create(new Uri(Constants.SERVICE_URI, url)) as HttpWebRequest;
			request.Method = "GET";
			ForceHeader(request, "Range", $"bytes=asdf");
			try {
				var response = (HttpWebResponse)request.GetResponse();
				Assert.AreEqual(HttpStatusCode.RequestedRangeNotSatisfiable, response.StatusCode);
			}
			catch (WebException e) {
				Assert.AreEqual(HttpStatusCode.RequestedRangeNotSatisfiable, ((HttpWebResponse)e.Response).StatusCode);
			}
		}

		[Test]
		public void TestGetAssetKnownTextureByteRangeWrongRangeFormat_3_RequestedRangeNotSatisfiable() {
			// Aperture has this block commented out, resulting in the error to match spec.
			var url = $"/CAPS/HTT/{_capId.ToString("N")}?texture_id={_knownTextureTGAAsset.Id.ToString("N")}";
			var request = WebRequest.Create(new Uri(Constants.SERVICE_URI, url)) as HttpWebRequest;
			request.Method = "GET";
			ForceHeader(request, "Range", $"bytes=3");
			try {
				var response = (HttpWebResponse)request.GetResponse();
				Assert.AreEqual(HttpStatusCode.RequestedRangeNotSatisfiable, response.StatusCode);
			}
			catch (WebException e) {
				Assert.AreEqual(HttpStatusCode.RequestedRangeNotSatisfiable, ((HttpWebResponse)e.Response).StatusCode);
			}
		}

		[Test]
		public void TestGetAssetKnownTextureByteRangeWrongUnitType_asdf_Ok() { // As per RFC7233: "An origin server MUST ignore a Range header field that contains a range unit it does not understand."
			var url = $"/CAPS/HTT/{_capId.ToString("N")}?texture_id={_knownTextureTGAAsset.Id.ToString("N")}";
			var request = WebRequest.Create(new Uri(Constants.SERVICE_URI, url)) as HttpWebRequest;
			request.Method = "GET";
			ForceHeader(request, "Range", $"asdf=0-0");
			try {
				var response = (HttpWebResponse)request.GetResponse();
				Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);
			}
			catch (WebException e) {
				Assert.AreEqual(HttpStatusCode.OK, ((HttpWebResponse)e.Response).StatusCode);
			}
		}

		[Test]
		public void TestGetAssetKnownTextureByteRange_8dash3_RequestedRangeNotSatisfiable() {
			var url = $"/CAPS/HTT/{_capId.ToString("N")}?texture_id={_knownTextureTGAAsset.Id.ToString("N")}";
			var request = WebRequest.Create(new Uri(Constants.SERVICE_URI, url)) as HttpWebRequest;
			request.Method = "GET";
			ForceHeader(request, "Range", $"bytes=8-3");
			try {
				var response = (HttpWebResponse)request.GetResponse();
				Assert.AreEqual(HttpStatusCode.RequestedRangeNotSatisfiable, response.StatusCode);
			}
			catch (WebException e) {
				Assert.AreEqual(HttpStatusCode.RequestedRangeNotSatisfiable, ((HttpWebResponse)e.Response).StatusCode);
			}
		}

		[Test]
		public void TestGetAssetKnownTextureByteRange_dash3dash5_RequestedRangeNotSatisfiable() {
			var url = $"/CAPS/HTT/{_capId.ToString("N")}?texture_id={_knownTextureTGAAsset.Id.ToString("N")}";
			var request = WebRequest.Create(new Uri(Constants.SERVICE_URI, url)) as HttpWebRequest;
			request.Method = "GET";
			ForceHeader(request, "Range", $"bytes=-3-5");
			try {
				var response = (HttpWebResponse)request.GetResponse();
				Assert.AreEqual(HttpStatusCode.RequestedRangeNotSatisfiable, response.StatusCode);
			}
			catch (WebException e) {
				Assert.AreEqual(HttpStatusCode.RequestedRangeNotSatisfiable, ((HttpWebResponse)e.Response).StatusCode);
			}
		}

		[Test]
		public void TestGetAssetKnownTextureByteRange_0dashdash5_RequestedRangeNotSatisfiable() {
			var url = $"/CAPS/HTT/{_capId.ToString("N")}?texture_id={_knownTextureTGAAsset.Id.ToString("N")}";
			var request = WebRequest.Create(new Uri(Constants.SERVICE_URI, url)) as HttpWebRequest;
			request.Method = "GET";
			ForceHeader(request, "Range", $"bytes=0--5");
			try {
				var response = (HttpWebResponse)request.GetResponse();
				Assert.AreEqual(HttpStatusCode.RequestedRangeNotSatisfiable, response.StatusCode);
			}
			catch (WebException e) {
				Assert.AreEqual(HttpStatusCode.RequestedRangeNotSatisfiable, ((HttpWebResponse)e.Response).StatusCode);
			}
		}

		[Test]
		public void TestGetAssetKnownTextureByteRange_excess_RequestedRangeNotSatisfiable() {
			var url = $"/CAPS/HTT/{_capId.ToString("N")}?texture_id={_knownTextureTGAAsset.Id.ToString("N")}";
			var request = WebRequest.Create(new Uri(Constants.SERVICE_URI, url)) as HttpWebRequest;
			request.Method = "GET";
			ForceHeader(request, "Range", $"bytes=0-1,3-4,6-7,9-10,12-13,15-16");
			try {
				var response = (HttpWebResponse)request.GetResponse();
				Assert.AreEqual(HttpStatusCode.RequestedRangeNotSatisfiable, response.StatusCode);
			}
			catch (WebException e) {
				Assert.AreEqual(HttpStatusCode.RequestedRangeNotSatisfiable, ((HttpWebResponse)e.Response).StatusCode);
			}
		}

		#endregion

		#region Valid Ranges

		[Test]
		public void TestGetAssetKnownTextureByteRange_0dash0_ContentRangeCorrect() {
			var response = GetAsset(_capId, _knownTextureAsset.Id, ranges: new List<Range> { new Range(0, 0) });
			var contentRange = response.Headers.Get("Content-Range");
			Assert.AreEqual($"bytes 0-0/{_knownTextureAsset.Data.Length}", contentRange);
		}

		[Test]
		public async Task TestGetAssetKnownTextureByteRange_0dash0_CorrectData() {
			var response = GetAsset(_capId, _knownTextureAsset.Id, ranges: new List<Range> { new Range(0, 0) });
			var assetData = new byte[_knownTextureAsset.Data.Length];
			var bytesRead = await response.GetResponseStream().ReadAsync(assetData, 0, assetData.Length);
			Assert.AreEqual(_knownTextureAsset.Data.Take(1), assetData.Take(bytesRead));
		}

		[Test]
		public void TestGetAssetKnownTextureByteRange_0dash0_PartialContent() {
			var response = GetAsset(_capId, _knownTextureAsset.Id, ranges: new List<Range> { new Range(0, 0)});
			Assert.AreEqual(HttpStatusCode.PartialContent, response.StatusCode);
		}

		[Test]
		public async Task TestGetAssetKnownTextureByteRange_0dash_CorrectData() {
			var response = GetAsset(_capId, _knownTextureAsset.Id, ranges: new List<Range> { new Range(0, null) });
			var assetData = new byte[_knownTextureAsset.Data.Length];
			var bytesRead = await response.GetResponseStream().ReadAsync(assetData, 0, assetData.Length);
			Assert.AreEqual(_knownTextureAsset.Data, assetData.Take(bytesRead));
		}

		[Test]
		public void TestGetAssetKnownTextureByteRange_0dash_Ok() {
			var response = GetAsset(_capId, _knownTextureAsset.Id, ranges: new List<Range> { new Range(0, null) });
			Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);
		}

		[Test]
		public void TestGetAssetKnownTextureByteRange_0dash4_ContentRangeCorrect() {
			var response = GetAsset(_capId, _knownTextureAsset.Id, ranges: new List<Range> { new Range(0, 4) });
			var contentRange = response.Headers.Get("Content-Range");
			Assert.AreEqual($"bytes 0-4/{_knownTextureAsset.Data.Length}", contentRange);
		}

		[Test]
		public async Task TestGetAssetKnownTextureByteRange_0dash4_CorrectData() {
			var response = GetAsset(_capId, _knownTextureAsset.Id, ranges: new List<Range> { new Range(0, 4) });
			var assetData = new byte[_knownTextureAsset.Data.Length];
			var bytesRead = await response.GetResponseStream().ReadAsync(assetData, 0, assetData.Length);
			Assert.AreEqual(_knownTextureAsset.Data.Take(5), assetData.Take(bytesRead));
		}

		[Test]
		public void TestGetAssetKnownTextureByteRange_0dash4_PartialContent() {
			var response = GetAsset(_capId, _knownTextureAsset.Id, ranges: new List<Range> { new Range(0, 4) });
			Assert.AreEqual(HttpStatusCode.PartialContent, response.StatusCode);
		}

		[Test]
		public void TestGetAssetKnownTextureByteRange_3dash_ContentRangeCorrect() {
			var response = GetAsset(_capId, _knownTextureAsset.Id, ranges: new List<Range> { new Range(3, null) });
			var contentRange = response.Headers.Get("Content-Range");
			Assert.AreEqual($"bytes 3-{_knownTextureAsset.Data.Length - 1}/{_knownTextureAsset.Data.Length}", contentRange.ToString());
		}

		[Test]
		public async Task TestGetAssetKnownTextureByteRange_3dash_CorrectData() {
			var response = GetAsset(_capId, _knownTextureAsset.Id, ranges: new List<Range> { new Range(3, null) });
			var assetData = new byte[_knownTextureAsset.Data.Length];
			var bytesRead = await response.GetResponseStream().ReadAsync(assetData, 0, assetData.Length);
			Assert.AreEqual(_knownTextureAsset.Data.Skip(3), assetData.Take(bytesRead));
		}

		[Test]
		public void TestGetAssetKnownTextureByteRange_3dash_PartialContent() {
			var response = GetAsset(_capId, _knownTextureAsset.Id, ranges: new List<Range> { new Range(3, null) });
			Assert.AreEqual(HttpStatusCode.PartialContent, response.StatusCode);
		}

		[Test]
		public void TestGetAssetKnownTextureByteRange_5dash5_ContentRangeCorrect() {
			var response = GetAsset(_capId, _knownTextureAsset.Id, ranges: new List<Range> { new Range(5, 5) });
			var contentRange = response.Headers.Get("Content-Range");
			Assert.AreEqual($"bytes 5-5/{_knownTextureAsset.Data.Length}", contentRange.ToString());
		}

		[Test]
		public async Task TestGetAssetKnownTextureByteRange_5dash5_CorrectData() {
			// One byte only.
			var response = GetAsset(_capId, _knownTextureAsset.Id, ranges: new List<Range> { new Range(5, 5) });
			var assetData = new byte[_knownTextureAsset.Data.Length];
			var bytesRead = await response.GetResponseStream().ReadAsync(assetData, 0, assetData.Length);
			Assert.AreEqual(_knownTextureAsset.Data.Skip(5).Take(1), assetData.Take(bytesRead));
		}

		[Test]
		public void TestGetAssetKnownTextureByteRange_5dash5_PartialContent() {
			var response = GetAsset(_capId, _knownTextureAsset.Id, ranges: new List<Range> { new Range(5, 5) });
			Assert.AreEqual(HttpStatusCode.PartialContent, response.StatusCode);
		}

		[Test]
		public void TestGetAssetKnownTextureByteRange_dash3_ContentRangeCorrect() {
			var response = GetAsset(_capId, _knownTextureAsset.Id, ranges: new List<Range> { new Range(null, -3) });
			var contentRange = response.Headers.Get("Content-Range");
			Assert.AreEqual($"bytes {_knownTextureAsset.Data.Length - 3}-{_knownTextureAsset.Data.Length - 1}/{_knownTextureAsset.Data.Length}", contentRange.ToString());
		}

		[Test]
		public async Task TestGetAssetKnownTextureByteRange_dash3_CorrectData() {
			// Aperture treats "-3" same as "0-3" which is wrong.
			// Correct RFC7233 page 6 result: the last 3 bytes.
			var response = GetAsset(_capId, _knownTextureAsset.Id, ranges: new List<Range> { new Range(null, -3) });
			var assetData = new byte[_knownTextureAsset.Data.Length];
			var bytesRead = await response.GetResponseStream().ReadAsync(assetData, 0, assetData.Length);
			Assert.AreEqual(_knownTextureAsset.Data.Skip(_knownTextureAsset.Data.Length - 3), assetData.Take(bytesRead));
		}

		[Test]
		public async Task TestGetAssetKnownTextureByteRange_dash3_Only3Bytes() {
			// Aperture treats "-3" same as "0-3" which is wrong.
			// Correct RFC7233 page 6 result: the last 3 bytes.
			var response = GetAsset(_capId, _knownTextureAsset.Id, ranges: new List<Range> { new Range(null, -3) });
			var assetData = new byte[_knownTextureAsset.Data.Length];
			var bytesRead = await response.GetResponseStream().ReadAsync(assetData, 0, assetData.Length);
			Assert.AreEqual(3, bytesRead);
		}

		[Test]
		public void TestGetAssetKnownTextureByteRange_dash3_PartialContent() {
			var response = GetAsset(_capId, _knownTextureAsset.Id, ranges: new List<Range> { new Range(null, -3) });
			Assert.AreEqual(HttpStatusCode.PartialContent, response.StatusCode);
		}

		[Test]
		public async Task TestGetAssetKnownTextureByteRange_dashMax_CorrectData() {
			var response = GetAsset(_capId, _knownTextureAsset.Id, ranges: new List<Range> { new Range(null, -_knownTextureAsset.Data.Length) });
			var assetData = new byte[_knownTextureAsset.Data.Length];
			var bytesRead = await response.GetResponseStream().ReadAsync(assetData, 0, assetData.Length);
			Assert.AreEqual(_knownTextureAsset.Data, assetData.Take(bytesRead));
		}

		[Test]
		public void TestGetAssetKnownTextureByteRange_dashMax_Ok() {
			var response = GetAsset(_capId, _knownTextureAsset.Id, ranges: new List<Range> { new Range(null, -_knownTextureAsset.Data.Length) });
			Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);
		}

		[Test]
		public async Task TestGetAssetKnownTextureByteRange_dash9999_CorrectData() {
			// Aperture limits to known byte range from asset.
			var response = GetAsset(_capId, _knownTextureAsset.Id, ranges: new List<Range> { new Range(null, -9999) });
			var assetData = new byte[_knownTextureAsset.Data.Length];
			var bytesRead = await response.GetResponseStream().ReadAsync(assetData, 0, assetData.Length);
			Assert.AreEqual(_knownTextureAsset.Data, assetData.Take(bytesRead));
		}

		[Test]
		public void TestGetAssetKnownTextureByteRange_dash9999_Ok() {
			var response = GetAsset(_capId, _knownTextureAsset.Id, ranges: new List<Range> { new Range(null, -9999) });
			Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);
		}

		[Test]
		public void TestGetAssetKnownTextureByteRange_5dash10_ContentRangeCorrect() {
			var response = GetAsset(_capId, _knownTextureAsset.Id, ranges: new List<Range> { new Range(5, 10) });
			var contentRange = response.Headers.Get("Content-Range");
			Assert.AreEqual($"bytes 5-10/{_knownTextureAsset.Data.Length}", contentRange.ToString());
		}

		[Test]
		public async Task TestGetAssetKnownTextureByteRange_5dash10_CorrectData() {
			var response = GetAsset(_capId, _knownTextureAsset.Id, ranges: new List<Range> { new Range(5, 10) });
			var assetData = new byte[_knownTextureAsset.Data.Length];
			var bytesRead = await response.GetResponseStream().ReadAsync(assetData, 0, assetData.Length);
			Assert.AreEqual(_knownTextureAsset.Data.Skip(5).Take(1 + 10 - 5), assetData.Take(bytesRead));
		}

		[Test]
		public void TestGetAssetKnownTextureByteRange_5dash10_PartialContent() {
			var response = GetAsset(_capId, _knownTextureAsset.Id, ranges: new List<Range> { new Range(5, 10) });
			Assert.AreEqual(HttpStatusCode.PartialContent, response.StatusCode);
		}

		[Test]
		public async Task TestGetAssetKnownTextureByteRange_0dash9999_CorrectData() {
			// Aperture limits to known byte range from asset.
			var response = GetAsset(_capId, _knownTextureAsset.Id, ranges: new List<Range> { new Range(0, 9999) });
			var assetData = new byte[_knownTextureAsset.Data.Length];
			var bytesRead = await response.GetResponseStream().ReadAsync(assetData, 0, assetData.Length);
			Assert.AreEqual(_knownTextureAsset.Data.Take(9999), assetData.Take(bytesRead));
		}

		[Test]
		public void TestGetAssetKnownTextureByteRange_0dash9999_Ok() {
			var response = GetAsset(_capId, _knownTextureAsset.Id, ranges: new List<Range> { new Range(0, 9999) });
			Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);
		}

		[Test]
		public void TestGetAssetKnownTextureByteRange_5dash9999_ContentRangeCorrect() {
			var response = GetAsset(_capId, _knownTextureAsset.Id, ranges: new List<Range> { new Range(5, 9999) });
			var contentRange = response.Headers.Get("Content-Range");
			Assert.AreEqual($"bytes 5-{_knownTextureAsset.Data.Length - 1}/{_knownTextureAsset.Data.Length}", contentRange.ToString());
		}

		[Test]
		public async Task TestGetAssetKnownTextureByteRange_5dash9999_CorrectData() {
			// Aperture limits to known byte range from asset.
			var response = GetAsset(_capId, _knownTextureAsset.Id, ranges: new List<Range> { new Range(5, 9999) });
			var assetData = new byte[_knownTextureAsset.Data.Length];
			var bytesRead = await response.GetResponseStream().ReadAsync(assetData, 0, assetData.Length);
			Assert.AreEqual(_knownTextureAsset.Data.Skip(5).Take(1 + 9999 - 5), assetData.Take(bytesRead));
		}

		[Test]
		public void TestGetAssetKnownTextureByteRange_5dash9999_PartialContent() {
			var response = GetAsset(_capId, _knownTextureAsset.Id, ranges: new List<Range> { new Range(5, 9999) });
			Assert.AreEqual(HttpStatusCode.PartialContent, response.StatusCode);
		}

		[Test] // I decided that this server should respond with the single-range type response if the multipart request coalesced into a single range.
		public async Task TestGetAssetKnownTextureByteRange_0dash1_1dash_dash1_CorrectData() {
			var response = GetAsset(_capId, _knownTextureAsset.Id, ranges: new List<Range> { new Range(0, 1), new Range(1, null), new Range(null, -1) });
			var assetData = new byte[_knownTextureAsset.Data.Length];
			var bytesRead = await response.GetResponseStream().ReadAsync(assetData, 0, assetData.Length);
			Assert.AreEqual(_knownTextureAsset.Data, assetData.Take(bytesRead));
		}

		[Test]
		public void TestGetAssetKnownTextureByteRange_0dash1_1dash_dash1_Ok() {
			var response = GetAsset(_capId, _knownTextureAsset.Id, ranges: new List<Range> { new Range(0, 1), new Range(1, null), new Range(null, -1) });
			Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);
		}

		[Test]
		public void TestGetAssetKnownTextureByteRange_0dash1_5dash8_CorrectMediaType() {
			var response = GetAsset(_capId, _knownTextureAsset.Id, ranges: new List<Range> { new Range(0, 1), new Range(5, 8) });
			Assert.That(response.ContentType.StartsWith("multipart/byteranges; boundary=", StringComparison.Ordinal));
		}

		[Test]
		public async Task TestGetAssetKnownTextureByteRange_0dash1_5dash8_CorrectMimeTypes() {
			var response = GetAsset(_capId, _knownTextureAsset.Id, ranges: new List<Range> { new Range(0, 1), new Range(5, 8) });

			using (var stream = response.GetResponseStream())
			using (var contentStream = new StreamContent(stream)) {
				contentStream.Headers.ContentType = System.Net.Http.Headers.MediaTypeHeaderValue.Parse(response.ContentType);
				var content = await contentStream.ReadAsMultipartAsync();

				foreach (var section in content.Contents) {
					var contentType = section.Headers.ContentType;
					Assert.AreEqual("image/x-j2c", contentType.MediaType);
				}
			}
		}

		[Test]
		public async Task TestGetAssetKnownTextureByteRange_0dash1_5dash8_CorrectContentRanges() {
			var response = GetAsset(_capId, _knownTextureAsset.Id, ranges: new List<Range> { new Range(0, 1), new Range(5, 8) });

			using (var stream = response.GetResponseStream())
			using (var contentStream = new StreamContent(stream)) {
				contentStream.Headers.ContentType = System.Net.Http.Headers.MediaTypeHeaderValue.Parse(response.ContentType);
				var content = await contentStream.ReadAsMultipartAsync();

				Assert.AreEqual(2, content.Contents.Count());
				var section1 = content.Contents.First();
				var contentRange1 = section1.Headers.ContentRange;
				Assert.AreEqual($"bytes 0-1/{_knownTextureAsset.Data.Length}", contentRange1.ToString());

				var section2 = content.Contents.Skip(1).First();
				var contentRange2 = section2.Headers.ContentRange;
				Assert.AreEqual($"bytes 5-8/{_knownTextureAsset.Data.Length}", contentRange2.ToString());
			}
		}

		[Test]
		public async Task TestGetAssetKnownTextureByteRange_0dash1_5dash8_CorrectData() {
			var response = GetAsset(_capId, _knownTextureAsset.Id, ranges: new List<Range> { new Range(0, 1), new Range(5, 8) });

			using (var stream = response.GetResponseStream())
			using (var contentStream = new StreamContent(stream)) {
				contentStream.Headers.ContentType = System.Net.Http.Headers.MediaTypeHeaderValue.Parse(response.ContentType);
				var content = await contentStream.ReadAsMultipartAsync();

				Assert.AreEqual(2, content.Contents.Count());
				var section1 = content.Contents.First();
				Assert.AreEqual(_knownTextureAsset.Data.Take(2), await section1.ReadAsByteArrayAsync());

				var section2 = content.Contents.Skip(1).First();
				Assert.AreEqual(_knownTextureAsset.Data.Skip(5).Take(8 - 5 + 1), await section2.ReadAsByteArrayAsync());
			}
		}

		[Test]
		public void TestGetAssetKnownTextureByteRange_0dash1_5dash8_PartialContent() {
			var response = GetAsset(_capId, _knownTextureAsset.Id, ranges: new List<Range> { new Range(0, 1), new Range(5, 8) });
			Assert.AreEqual(HttpStatusCode.PartialContent, response.StatusCode);
		}

		#endregion

		// TODO: figure out a way to test bandwidth limiting.

		[Test]
		public void TestGetAssetUnknownCapNotFound() {
			try {
				var response = GetAsset(Guid.NewGuid(), _knownTextureAsset.Id);
				Assert.AreEqual(HttpStatusCode.NotFound, response.StatusCode);
			}
			catch (WebException e) {
				Assert.AreEqual(HttpStatusCode.NotFound, ((HttpWebResponse)e.Response).StatusCode);
			}
		}

		[Test]
		public void TestGetAssetUnknownAssetNotFound() {
			try {
				var response = GetAsset(_capId, Guid.NewGuid());
				Assert.AreEqual(HttpStatusCode.NotFound, response.StatusCode);
			}
			catch (WebException e) {
				Assert.AreEqual(HttpStatusCode.NotFound, ((HttpWebResponse)e.Response).StatusCode);
			}
		}
	}
}
