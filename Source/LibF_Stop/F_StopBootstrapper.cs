// F_StopBootstrapper.cs
//
// Author:
//       ricky <>
//
// Copyright (c) 2017 ${CopyrightHolder}
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
using System.Text;
using Nancy;
using Nancy.ErrorHandling;

namespace LibF_Stop {
	// Automagically called by the default bootstrapper.
	public class F_StopBootstrapper : DefaultNancyBootstrapper {
		private static readonly log4net.ILog LOG = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

		protected override void ApplicationStartup(Nancy.TinyIoc.TinyIoCContainer container, Nancy.Bootstrapper.IPipelines pipelines) {
			base.ApplicationStartup(container, pipelines);

			pipelines.OnError += (context, exception) => {
				LOG.Error($"Unhandled error from '{context.Request.UserHostAddress}' on '{context.Request.Url}': {exception.Message}", exception);

				var response = new Response {
					StatusCode = HttpStatusCode.InternalServerError,
					ContentType = "application/json",
					Contents = (obj) => {
						var output = Encoding.UTF8.GetBytes("{\"error\": \"Server error.  This error has been sent to the developers.\"}");
						obj.Write(output, 0, output.Length);
					}
				};
				return response;
			};

#if DEBUG
			StaticConfiguration.DisableErrorTraces = false;
#endif
		}
	}
}