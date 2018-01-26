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
using System.Linq;
using System.Text;
using ChattelAssetTools;
using InWorldz.Data.Assets.Stratus;
using Nancy;

namespace LibF_Stop {
	public sealed class F_StopRouter : NancyModule {
		private static readonly log4net.ILog LOG = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

		// Why static for the cap admin? because this router class is instanced automatically to handle requests.
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

				var rangeHeaderVals = Request.Headers["Range"];
				string rangeHeader = null;
				if (rangeHeaderVals.Any()) {
					rangeHeader = rangeHeaderVals.Aggregate((prev, next) => $"{prev},{next}"); // Because Nancy's being too smart.
				}
				IEnumerable<Range> ranges = null;

				// Parse and store the ranges, but only if a byte range. As per RFC7233: "An origin server MUST ignore a Range header field that contains a range unit it does not understand."
				if (rangeHeader?.StartsWith("bytes=", StringComparison.Ordinal) ?? false) {
					try {
						ranges = Range.ParseRanges(rangeHeader);
					}
					catch (FormatException) {
						LOG.Warn($"Bad range header for asset from {Request.UserHostAddress}. Requested header doesn't match RFC7233: {rangeHeader}");
						return StockReply.RangeError;
					}
					catch (ArgumentOutOfRangeException e) {
						LOG.Warn($"Bad range header for asset from {Request.UserHostAddress}: {rangeHeader}", e);
						return StockReply.RangeError;
					}
				}

				if ((ranges?.Count() ?? 0) > 5) { // 5 is arbitrary.  In reality ranges should be few, lots of ranges usually mean an attack.
					LOG.Warn($"Too many ranges requested from {Request.UserHostAddress}: {rangeHeader}");
					return StockReply.RangeError;
				}

				var completionSource = new System.Threading.Tasks.TaskCompletionSource<Response>();

				AssetRequest.AssetErrorHandler errorHandler = error => {
					switch (error.Error) {
						case AssetErrorType.CapabilityIdUnknown:
							LOG.Warn($"Request on nonexistent cap from {Request.UserHostAddress} {error.Message}");
							completionSource.SetResult(StockReply.NotFound);
						break;
						case AssetErrorType.AssetTypeWrong:
							LOG.Warn($"Request for wrong kind of asset from {Request.UserHostAddress} {error.Message}");
							completionSource.SetResult(StockReply.BadRequest);
						break;
						case AssetErrorType.ConfigIncorrect:
							LOG.Warn($"Configuration incomplete! {error.Message}");
							completionSource.SetResult(StockReply.InternalServerError);
						break;
						case AssetErrorType.AssetIdUnknown:
							LOG.Warn($"Request for unknown asset from {Request.UserHostAddress} {error.Message}");
							completionSource.SetResult(StockReply.NotFound);
						break;
						case AssetErrorType.QueueFilled:
							LOG.Warn($"Request from {Request.UserHostAddress} had to be dropped because cap {_.capId} is paused and filled. {error.Message}");
							completionSource.SetResult(StockReply.NotFound);
						break;
						default:
							LOG.Warn($"Request from {Request.UserHostAddress} had unexpected error! {error.Message}");
							completionSource.SetResult(StockReply.InternalServerError);
						break;
					}
				};

				try {
					if (textureId != null) {
						_capAdmin.RequestTextureAssetOnCap(
							(Guid)_.capId,
							(Guid)textureId,
							asset => {
								var response = new Response();

								PrepareResponse(response, asset, ranges);

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

								PrepareResponse(response, asset, ranges);

								completionSource.SetResult(response);
							},
							errorHandler
						);
					}
				}
				catch (IndexOutOfRangeException e) {
					LOG.Warn($"Bad range requested from {Request.UserHostAddress} for asset {textureId ?? meshId}: {rangeHeader}", e);
					return StockReply.RangeError;
				}

				return await completionSource.Task;
			};
		}

		private void PrepareResponse(Response response, StratusAsset asset, IEnumerable<Range> ranges) {
			if (ranges != null) {
				// Sorting the ranges breaks RFC7233 Section 4.1's "the server SHOULD send the parts in the same order" that it got them,
				// but it also notes "a client cannot rely on receiving ... the same order that it requested."
				// The spec also says "A client that is requesting multiple ranges SHOULD list those ranges in ascending order..."
				// Thus I can sort how I deem fit, and I'm gonna.  Ask crazy byte orders expect a sane response or none at all.
				ranges = Range.SortAndCoalesceRanges(ranges, asset.Data.Length);

				if (ranges.Count() == 1) {
					var range = ranges.First();
					if (range.Min == 0 && range.Max == asset.Data.Length - 1) {
						ranges = null; // Same as if there was no range header sent.
					}
				}
			}

			var contentType = GetContentType(asset);

			if (ranges == null) { // Everything was requested, so send it all!
				response.ContentType = contentType;
				response.StatusCode = HttpStatusCode.OK;
				response.Contents = stream => stream.Write(asset.Data, 0, asset.Data.Length);
			}
			else if (ranges.Count() == 1) { // Single part partial response
				var range = ranges.First();

				response.ContentType = contentType;
				response.StatusCode = HttpStatusCode.PartialContent;
				response.Headers.Add("Content-Range", $"bytes {range.Min}-{range.Max}/{asset.Data.Length}");
				response.Contents = stream => stream.Write(asset.Data, (int)range.Min, (int)(range.Max - range.Min + 1));
			}
			else { // Multipart partial response
				// Separator is a GUID with some constant strings to make it easier to see by humans and less likely to collide with any values in the data.
				var boundary = $"++++{Guid.NewGuid().ToString("N")}++++";

				response.ContentType = $"multipart/byteranges; boundary={boundary}";
				response.StatusCode = HttpStatusCode.PartialContent;

				// AFAICT NancyFX doesn't have any built-in way to send multipart/byteranges.  I have to do this myself.

				void writeString(Stream stream, string str, Encoding encoding) {
					var bytes = encoding.GetBytes(str);
					stream.Write(bytes, 0, bytes.Length);
				}

				response.Contents = stream => {
					writeString(stream, $"\r\n", Encoding.ASCII); // Because http://www.underealm.com/code/2015/08/rfc7233-is-wrong/
					foreach (var range in ranges) {
						writeString(stream, $"\r\n--{boundary}\r\nContent-Type: {contentType}\r\nContent-Range: bytes {range.Min}-{range.Max}/{asset.Data.Length}\r\n\r\n", Encoding.ASCII);
						stream.Write(asset.Data, (int)range.Min, (int)(range.Max - range.Min + 1));
					}
					writeString(stream, $"\r\n--{boundary}-- \r\n", Encoding.ASCII);
				};
			}
		}

		private string GetContentType(StratusAsset asset) {
			if (asset.IsImageAsset()) {
				if (Encoding.ASCII.GetString(asset.Data, 0, Math.Min(asset.Data.Length, JPEG2000_MAGIC_NUMBERS.Length)).Equals(JPEG2000_MAGIC_NUMBERS, StringComparison.Ordinal)) {
					return "image/x-j2c";
				}

				if (Encoding.ASCII.GetString(asset.Data, 0, Math.Min(asset.Data.Length, JPEG_MAGIC_NUMBERS.Length)).Equals(JPEG_MAGIC_NUMBERS, StringComparison.Ordinal)) {
					return "image/jpeg";
				}

				return "image/x-tga";
			}

			// else Mesh
			return "application/vnd.ll.mesh";
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
