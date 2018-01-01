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
using Chattel;
using Nancy;

namespace LibF_Stop {
	public class F_StopRouter : NancyModule {
		private static readonly log4net.ILog LOG = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

		// TODO: Make sure to implement a negative cache for items that are requested but are not allowed types.  Such requests should be logged with the client details so that fail2ban can catch it.

		public F_StopRouter() : base("/CAPS/HTT") {
			var capAdmin = new CapAdministration();

			Get["/TEST"] = _ => {
				LOG.Debug($"Test called by IP '{Request.UserHostAddress}'.");

				return (Response)"OK";
			};

			Get["/ADDCAP/{adminToken}/{capId:guid}/{bandwidth?:min(0)}"] = _ => {
				uint bandwidth = 0;
				if (_.bandwidth != null && (int)_.bandwidth >= 0) {
					bandwidth = _.bandwidth;
				}

				var result = capAdmin.AddCap(_.adminToken, _.capId, bandwidth);

				return (Response)""; // TODO find out response....
			};

			Get["/REMCAP/{adminToken}/{capId:guid}"] = _ => {
				var result = capAdmin.RemoveCap(_.adminToken, _.capId);

				return (Response)""; // TODO find out response....
			};

			Get["/PAUSE/{adminToken}/{capId:guid}"] = _ => {
				var result = capAdmin.PauseCap(_.adminToken, _.capId);

				return (Response)""; // TODO find out response....
			};

			Get["/RESUME/{adminToken}/{capId:guid}"] = _ => {
				var result = capAdmin.ResumeCap(_.adminToken, _.capId);

				return (Response)""; // TODO find out response....
			};

			Get["/LIMIT/{adminToken}/{capId:guid}/{bandwidth?:min(0)}"] = _ => {
				uint bandwidth = 0;
				if (_.bandwidth != null && (int)_.bandwidth >= 0) {
					bandwidth = _.bandwidth;
				}

				var result = capAdmin.LimitCap(_.adminToken, _.capId, bandwidth);

				return (Response)""; // TODO find out response....
			};

			Get["/{capId:guid}"] = _ => {
				// TODO: split along texture/mesh thoughts
				var textureId = (Guid?)Request.Query["texture_id"];
				var meshId = (Guid?)Request.Query["mesh_id"];

				if (textureId == null && meshId == null) {
					
				}

				try {
					//var result = capAdmin.LimitCap(_.adminToken, _.capId, bandwidth);

					return (Response)"";
				}
				catch (Exception e) {
					return (Response)"";
				}
			};

			// /CAPS/HTT/ADDCAP/{TOKEN}/{CAP_UUID}/{BANDWIDTH}
			// /CAPS/HTT/REMCAP/{TOKEN}/{CAP_UUID}
			// /CAPS/HTT/PAUSE/{TOKEN}/{CAP_UUID}
			// /CAPS/HTT/RESUME/{TOKEN}/{CAP_UUID}
			// /CAPS/HTT/LIMIT/{TOKEN}/{CAP_UUID}/{BANDWIDTH}
			// /CAPS/HTT/{CAP_UUID}/?texture_id={TEXTUREUUID}
			// /CAPS/HTT/{CAP_UUID}/?mesh_id={TEXTUREUUID}

		}
	}
}
