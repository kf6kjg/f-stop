// TestResumeCap.cs
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
using System.IO;
using System.Net;
using InWorldz.Data.Assets.Stratus;
using NUnit.Framework;
using RestSharp;

namespace f_stopHttpApiTests {
	[TestFixture]
	public class TestResumeCap {
		private StratusAsset _knownTextureAsset;

		public static IRestResponse ResumeCap(Guid capId, string adminToken = Constants.SERVICE_ADMIN_TOKEN) {
			var client = new RestClient(Constants.SERVICE_URI);
			var url = $"/CAPS/HTT/RESUME/{adminToken}/{capId.ToString("N")}";
			var request = new RestRequest(url, Method.GET);
			var response = client.Execute(request);
			return response;
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

			// Using hardcoded GUIDs to make debugging easier.

			_knownTextureAsset = TestGetAsset.CreateAndCacheAsset(
				"_knownTextureAsset",
				0,
				new byte[] { 0x00, 0x00, 0x00, 0x0C, 0x6A, 0x50, 0x20, 0x20, 0x0D, 0x0A, 0x87, 0x0A }, // JPEG-2000 magic numbers
				Guid.Parse("01000000-0000-0000-0000-000000000000")
			);
		}

		[Test]
		public void TestResumeCapAllowsGet() {
			var capId = Guid.NewGuid();
			TestAddCap.AddCap(capId);
			TestPauseCap.PauseCap(capId);
			ResumeCap(capId);
			try {
				var response = TestGetAsset.GetAsset(capId, _knownTextureAsset.Id, timeout: TimeSpan.FromMilliseconds(100));
				Assert.Pass();
			}
			catch (WebException e) {
				Assert.AreEqual(WebExceptionStatus.Timeout, e.Status); // It timed out
			}
		}

		[Test]
		public void TestResumeCapBadAdminTokenBadRequest() {
			var capId = Guid.NewGuid();
			TestAddCap.AddCap(capId);
			TestPauseCap.PauseCap(capId);
			var response = ResumeCap(capId, "badToken");
			Assert.AreEqual(HttpStatusCode.BadRequest, response.StatusCode, "Bad Status:\n\n" + response.Content);
		}

		[Test]
		public void TestResumeCapNotPausedOk() {
			var capId = Guid.NewGuid();
			TestAddCap.AddCap(capId);
			var response = ResumeCap(capId);
			Assert.AreEqual(HttpStatusCode.OK, response.StatusCode, "Bad Status:\n\n" + response.Content);
		}

		[Test]
		public void TestResumeCapPausedOk() {
			var capId = Guid.NewGuid();
			TestAddCap.AddCap(capId);
			TestPauseCap.PauseCap(capId);
			var response = ResumeCap(capId);
			Assert.AreEqual(HttpStatusCode.OK, response.StatusCode, "Bad Status:\n\n" + response.Content);
		}

		[Test]
		public void TestResumeCapUnknownCapBadRequest() {
			var response = ResumeCap(Guid.NewGuid());
			Assert.AreEqual(HttpStatusCode.BadRequest, response.StatusCode, "Bad Status:\n\n" + response.Content);
		}
	}
}
