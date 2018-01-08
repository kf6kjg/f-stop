// F_StopRouter.cs
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
using System.Collections.Generic;
using System.IO;
using System.Text;
using Nancy;

namespace LibF_Stop {
	public sealed class F_StopRouter : NancyModule {
		private static readonly log4net.ILog LOG = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

		private static CapAdministration _capAdmin = new CapAdministration();

		private static readonly string JPEG2000_MAGIC_NUMBERS = Encoding.ASCII.GetString(new byte[] { 0x00, 0x00, 0x00, 0x0C, 0x6A, 0x50, 0x20, 0x20, 0x0D, 0x0A, 0x87, 0x0A });
		private static readonly string JPEG_MAGIC_NUMBERS = Encoding.ASCII.GetString(new byte[] { 0xFF, 0xD8, 0xFF, 0xE1, 0x00, 0x00, 0x45, 0x78, 0x69, 0x66, 0x00 });

		public F_StopRouter() : base("/CAPS/HTT") {
			Get["/TEST"] = _ => {
				LOG.Debug($"Test called by {Request.UserHostAddress}");

				return Response.AsText("OK");
			};

			Get["/ADDCAP/{adminToken}/{capId:guid}/{bandwidth?}"] = _ => {
				if (_.bandwidth != null && (int)_.bandwidth < 0) {
					LOG.Warn($"Invalid bandwidth spec from {Request.UserHostAddress} on cap {_.capId}: bandwidth cannot be negative ({_.bandwidth})");
					return StockReply.BadRequest;
				}

				uint bandwidth = 0;
				if (_.bandwidth != null) {
					bandwidth = _.bandwidth;
				}

				try {
					var result = _capAdmin.AddCap(_.adminToken, _.capId, bandwidth);

					return result ? StockReply.Ok : StockReply.BadRequest;
				}
				catch (InvalidAdminTokenException) {
					LOG.Warn($"Invalid admin token from {Request.UserHostAddress}");
				}

				return StockReply.BadRequest;
			};

			Get["/REMCAP/{adminToken}/{capId:guid}"] = _ => {
				try {
					var result = _capAdmin.RemoveCap(_.adminToken, _.capId);

					return result ? StockReply.Ok : StockReply.BadRequest;
				}
				catch (InvalidAdminTokenException) {
					LOG.Warn($"Invalid admin token from {Request.UserHostAddress}");
				}

				return StockReply.BadRequest;
			};

			Get["/PAUSE/{adminToken}/{capId:guid}"] = _ => {
				try {
					var result = _capAdmin.PauseCap(_.adminToken, _.capId);

					return result ? StockReply.Ok : StockReply.BadRequest;
				}
				catch (InvalidAdminTokenException) {
					LOG.Warn($"Invalid admin token from {Request.UserHostAddress}");
				}

				return StockReply.BadRequest;
			};

			Get["/RESUME/{adminToken}/{capId:guid}"] = _ => {
				try {
					var result = _capAdmin.ResumeCap(_.adminToken, _.capId);

					return result ? StockReply.Ok : StockReply.BadRequest;
				}
				catch (InvalidAdminTokenException) {
					LOG.Warn($"Invalid admin token from {Request.UserHostAddress}");
				}

				return StockReply.BadRequest;
			};

			Get["/LIMIT/{adminToken}/{capId:guid}/{bandwidth?}"] = _ => {
				if (_.bandwidth != null && (int)_.bandwidth < 0) {
					LOG.Warn($"Invalid bandwidth spec from {Request.UserHostAddress} on cap {_.capId}: bandwidth cannot be negative ({_.bandwidth})");
					return StockReply.BadRequest;
				}

				uint bandwidth = 0;
				if (_.bandwidth != null) {
					bandwidth = _.bandwidth;
				}

				try {
					var result = _capAdmin.LimitCap(_.adminToken, _.capId, bandwidth);

					return result ? StockReply.Ok : StockReply.BadRequest;
				}
				catch (InvalidAdminTokenException) {
					LOG.Warn($"Invalid admin token from {Request.UserHostAddress}");
				}

				return StockReply.BadRequest;
			};

			Get["/{capId:guid}", true] = async (_, ct) => {
				var textureId = (Guid?)Request.Query["texture_id"];
				var meshId = (Guid?)Request.Query["mesh_id"];

				if (textureId == null && meshId == null) {
					LOG.Warn($"Bad request for asset from {Request.UserHostAddress}: mesh_id nor texture_id supplied");
					return StockReply.BadRequest;
				}

				if (textureId != null && meshId != null) {
					// Difference from Aperture: Aperture continues and only uses the texture_id when both are specc'd.
					LOG.Warn($"Bad request for asset from {Request.UserHostAddress}: both mesh_id and texture_id supplied");
					return StockReply.BadRequest;
				}

				if (textureId == Guid.Empty || meshId == Guid.Empty) {
					var type = textureId != null ? "texture" : "mesh";
					LOG.Warn($"Bad request for asset from {Request.UserHostAddress}: requested {type} is a zero guid.");
					return StockReply.BadRequest;
				}

				var completionSource = new System.Threading.Tasks.TaskCompletionSource<Response>();

				AssetRequest.AssetErrorHandler errorHandler = error => {
					if (error is InvalidCapabilityIdException) {
						LOG.Warn($"Request on nonexistent cap from {Request.UserHostAddress}", error);
						completionSource.SetResult(StockReply.NotFound);
					}
					else if (error is WrongAssetTypeException) {
						LOG.Warn($"Request for wrong kind of asset from {Request.UserHostAddress}", error);
						completionSource.SetResult(StockReply.BadRequest);
					}
					else if (error is ConfigException) {
						LOG.Warn($"Configuration incomplete!", error);
						completionSource.SetResult(StockReply.InternalServerError);
					}
					else if (error is AssetIdUnknownException) {
						LOG.Warn($"Request for unknown asset from {Request.UserHostAddress}", error);
						completionSource.SetResult(StockReply.NotFound);
					}
					else if (error is CapQueueFilledException) {
						LOG.Warn($"Request from {Request.UserHostAddress} had to be dropped because cap {_.capId} is paused and filled", error);
						completionSource.SetResult(StockReply.NotFound);
					}
					else {
						LOG.Warn($"Request from {Request.UserHostAddress} had unexpected error!", error);
						completionSource.SetResult(StockReply.InternalServerError);
					}
				};

				if (textureId != null) {
					_capAdmin.RequestTextureAssetOnCap(
						(Guid)_.capId,
						(Guid)textureId,
						asset => {
							var response = new Response();
							var data = asset.Data;

							if (Encoding.ASCII.GetString(data, 0, Math.Min(data.Length, JPEG2000_MAGIC_NUMBERS.Length)).Equals(JPEG2000_MAGIC_NUMBERS, StringComparison.Ordinal)) {
								response.ContentType = "image/x-j2c";
							}
							else if (Encoding.ASCII.GetString(data, 0, Math.Min(data.Length, JPEG_MAGIC_NUMBERS.Length)).Equals(JPEG_MAGIC_NUMBERS, StringComparison.Ordinal)) {
								response.ContentType = "image/jpeg";
							}
							else {
								response.ContentType = "image/x-tga";
							}
							response.Contents = stream => stream.Write(data, 0, data.Length);

							completionSource.SetResult(response);
						},
						errorHandler
					);

				}
				else {
					_capAdmin.RequestMeshAssetOnCap(
						(Guid)_.capId,
						(Guid)meshId,
						asset => {
							var response = new Response();
							var data = asset.Data;

							response.ContentType = "application/vnd.ll.mesh";
							response.Contents = stream => stream.Write(data, 0, data.Length);

							completionSource.SetResult(response);
						},
						errorHandler
					);
				}

				return await completionSource.Task;
			};
		}

		private static class StockReply {
			public static Response Ok = new Response {
				ContentType = "text/html",
				StatusCode = HttpStatusCode.OK,
				//Contents = stream => (new System.IO.StreamWriter(stream) { AutoFlush = true }).Write(@""),
				Headers = new Dictionary<string, string> {
					{"Content-Length", "0"},
				}
			};

			public static Response BadRequest = new Response {
				ContentType = "text/html",
				StatusCode = HttpStatusCode.BadRequest,
				Contents = stream => (new StreamWriter(stream) { AutoFlush = true }).Write(@"<html>
<head><title>Bad Request</title></head>
<body><h1>400 Bad Request</h1></body>
</html>"),
			};

			public static Response NotFound = new Response {
				ContentType = "text/html",
				StatusCode = HttpStatusCode.NotFound,
				Contents = stream => (new StreamWriter(stream) { AutoFlush = true }).Write(@"<html>
<head><title>Not Found</title></head>
<body><h1>404 Not Found</h1></body>
</html>"),
			};

			public static Response ServiceUnavailable = new Response {
				ContentType = "text/html",
				StatusCode = HttpStatusCode.ServiceUnavailable,
				Contents = stream => (new StreamWriter(stream) { AutoFlush = true }).Write(@"<html>
<head><title>Service Unavailable</title></head>
<body><h1>503 Service Unavailable</h1></body>
</html>"),
			};

			public static Response InternalServerError = new Response {
				ContentType = "text/html",
				StatusCode = HttpStatusCode.InternalServerError,
				Contents = stream => (new StreamWriter(stream) { AutoFlush = true }).Write(@"<html>
<head><title>Internal Server Error</title></head>
<body><h1>500 Internal Server Error</h1></body>
</html>"),
			};

			public static Response RangeError = new Response {
				ContentType = "text/html",
				StatusCode = HttpStatusCode.RequestedRangeNotSatisfiable,
				Contents = stream => (new StreamWriter(stream) { AutoFlush = true }).Write(@"<html>
<head><title>Requested Range not satisfiable</title></head>
<body><h1>416 Requested Range not satisfiable</h1></body>
</html>"),
			};
		}
	}
}
