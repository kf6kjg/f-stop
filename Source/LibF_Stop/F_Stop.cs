// F_Stop.cs
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

namespace LibF_Stop {
	public class F_Stop {
		private static readonly log4net.ILog LOG = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

		private readonly PIDFileManager _pidFileManager;

		private readonly string _address;
		private readonly uint _port;

		// TODO: Make sure to implement a negative cache for items that are requested but are not allowed types.  Such requests should be logged with the client details so that fail2ban can catch it.

		public F_Stop(string address, uint port, PIDFileManager pidFileManager, ChattelConfiguration chattelConfigRead) {
			if (address == null) {
				throw new ArgumentNullException(nameof(address));
			}
			if (pidFileManager == null) {
				throw new ArgumentNullException(nameof(pidFileManager));
			}
			if (chattelConfigRead == null) {
				throw new ArgumentNullException(nameof(chattelConfigRead));
			}
			LOG.Debug($"{address}:{port} - Initializing service.");

			_address = address;
			_port = port;

			_pidFileManager = pidFileManager;

			// TODO: set up chattel reader

			// TODO: figure out how to be a webservice.  Be sure to check out Anax2.
		}
	}
}
