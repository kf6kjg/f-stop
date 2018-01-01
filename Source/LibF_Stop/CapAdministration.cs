// CapAdministration.cs
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
using System.Collections.Concurrent;

namespace LibF_Stop {
	internal class CapAdministration {
		private ConcurrentDictionary<Guid, Capability> _caps;

		public bool AddCap(string adminToken, Guid id, uint bandwidth = 0) {
			return _caps.TryAdd(id, new Capability {
				BandwidthLimit = bandwidth,
				IsActive = true,
			});
		}

		public bool RemoveCap(string adminToken, Guid id) {
			if (_caps.TryRemove(id, out Capability cap)) {
				cap.IsActive = false;

				return true;
			}

			return false;
		}

		public bool PauseCap(string adminToken, Guid id) {
			if (_caps.TryGetValue(id, out Capability cap)) {
				cap.IsActive = false;

				return true;
			}

			return false;
		}

		public bool ResumeCap(string adminToken, Guid id) {
			if (_caps.TryGetValue(id, out Capability cap)) {
				cap.IsActive = true;

				return true;
			}

			return false;
		}

		public bool LimitCap(string adminToken, Guid id, uint bandwidth = 0) {
			if (_caps.TryGetValue(id, out Capability cap)) {
				cap.BandwidthLimit = bandwidth;

				return true;
			}

			return false;
		}

		public byte[] RequestTextureAssetOnCap(Guid capId, Guid assetId) {
			return null; // TODO
		}

		public byte[] RequestMeshAssetOnCap(Guid capId, Guid assetId) {
			return null; // TODO
		}
	}
}
