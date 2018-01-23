// TestAddCap.cs
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
using RestSharp;
using System.Net;

namespace f_stopHttpApiTests {
	[TestFixture]
	public class TestAddCap {
		public static IRestResponse AddCap(Guid capId, int? bandwidth = null, string adminToken = Constants.SERVICE_ADMIN_TOKEN) {
			var client = new RestClient(Constants.SERVICE_URI);
			var url = $"/CAPS/HTT/ADDCAP/{adminToken}/{capId.ToString("N")}" + (bandwidth != null ? $"/{bandwidth}" : "");
			var request = new RestRequest(url, Method.GET);
			var response = client.Execute(request);
			return response;
		}

		[Test]
		public void TestAddCapAddLimitedNegativeBadRequest() {
			var response = AddCap(Guid.NewGuid(), -1024);
			Assert.AreEqual(HttpStatusCode.BadRequest, response.StatusCode, "Bad Status:\n\n" + response.Content);
		}

		[Test]
		public void TestAddCapAddLimitedOk() {
			var response = AddCap(Guid.NewGuid(), 1024);
			Assert.AreEqual(HttpStatusCode.OK, response.StatusCode, "Bad Status:\n\n" + response.Content);
		}

		[Test]
		public void TestAddCapAddUnlimitedOk() {
			var response = AddCap(Guid.NewGuid());
			Assert.AreEqual(HttpStatusCode.OK, response.StatusCode, "Bad Status:\n\n" + response.Content);
		}

		[Test]
		public void TestAddCapDuplicateLimitedBadRequest() {
			var capId = Guid.NewGuid();
			AddCap(capId, 1024);
			var response = AddCap(capId, 1024);
			Assert.AreEqual(HttpStatusCode.BadRequest, response.StatusCode, "Bad Status:\n\n" + response.Content);
		}

		[Test]
		public void TestAddCapDuplicateUnlimitedBadRequest() {
			var capId = Guid.NewGuid();
			AddCap(capId);
			var response = AddCap(capId);
			Assert.AreEqual(HttpStatusCode.BadRequest, response.StatusCode, "Bad Status:\n\n" + response.Content);
		}

		[Test]
		public void TestAddCapWrongAdminTokenLimitedBadRequest() {
			var response = AddCap(Guid.NewGuid(), 1024, "wrong");
			Assert.AreEqual(HttpStatusCode.BadRequest, response.StatusCode, "Bad Status:\n\n" + response.Content);
		}

		[Test]
		public void TestAddCapWrongAdminTokenUnlimitedBadRequest() {
			var response = AddCap(Guid.NewGuid(), adminToken: "wrong");
			Assert.AreEqual(HttpStatusCode.BadRequest, response.StatusCode, "Bad Status:\n\n" + response.Content);
		}
	}
}
