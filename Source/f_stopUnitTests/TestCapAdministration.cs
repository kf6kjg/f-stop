// TestCapAdministration.cs
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

namespace f_stopUnitTests {
	[TestFixture]
	public class TestCapAdministration {
		private const string ADMIN_TOKEN = "test";

		private LibF_Stop.CapAdministration _capAdmin;

		[OneTimeSetUp]
		public void Setup() {
			LibF_Stop.ConfigSingleton.AdminToken = ADMIN_TOKEN;
			_capAdmin = new LibF_Stop.CapAdministration();
		}

		#region AddCap

		[Test]
		public void TestCapAdminAddCapLimitedDoesNotThrow() {
			Assert.DoesNotThrow(() => _capAdmin.AddCap(ADMIN_TOKEN, Guid.NewGuid(), 100000));
		}

		[Test]
		public void TestCapAdminAddCapLimitedReturnsTrue() {
			Assert.True(_capAdmin.AddCap(ADMIN_TOKEN, Guid.NewGuid(), 100000));
		}

		[Test]
		public void TestCapAdminAddCapLimitedTokenCaseChangeReturnsTrue() {
			Assert.True(_capAdmin.AddCap(ADMIN_TOKEN.ToUpper(), Guid.NewGuid(), 100000));
		}

		[Test]
		public void TestCapAdminAddCapLimitedWrongTokenReturnsFalse() {
			Assert.False(_capAdmin.AddCap("asdf", Guid.NewGuid(), 100000));
		}


		[Test]
		public void TestCapAdminAddCapUnlimitedDoesNotThrow() {
			Assert.DoesNotThrow(() => _capAdmin.AddCap(ADMIN_TOKEN, Guid.NewGuid()));
		}

		[Test]
		public void TestCapAdminAddCapUnlimitedReturnsTrue() {
			Assert.True(_capAdmin.AddCap(ADMIN_TOKEN, Guid.NewGuid()));
		}

		[Test]
		public void TestCapAdminAddCapUnlimitedWrongTokenReturnsFalse() {
			Assert.False(_capAdmin.AddCap("asdf", Guid.NewGuid()));
		}


		[Test]
		public void TestCapAdminAddCapLimitedDuplicateDoesNotThrow() {
			var capId = Guid.NewGuid();
			_capAdmin.AddCap(ADMIN_TOKEN, capId, 100000);
			Assert.DoesNotThrow(() => _capAdmin.AddCap(ADMIN_TOKEN, capId, 100000));
		}

		[Test]
		public void TestCapAdminAddCapLimitedDuplicateReturnsFalse() {
			var capId = Guid.NewGuid();
			_capAdmin.AddCap(ADMIN_TOKEN, capId, 100000);
			Assert.False(_capAdmin.AddCap(ADMIN_TOKEN, capId, 100000));
		}

		[Test]
		public void TestCapAdminAddCapLimitedDuplicateWrongTokenReturnsFalse() {
			var capId = Guid.NewGuid();
			_capAdmin.AddCap(ADMIN_TOKEN, capId, 100000);
			Assert.False(_capAdmin.AddCap("asdf", capId, 100000));
		}


		[Test]
		public void TestCapAdminAddCapUnlimitedDuplicateDoesNotThrow() {
			var capId = Guid.NewGuid();
			_capAdmin.AddCap(ADMIN_TOKEN, capId);
			Assert.DoesNotThrow(() => _capAdmin.AddCap(ADMIN_TOKEN, capId));
		}

		[Test]
		public void TestCapAdminAddCapUnlimitedDuplicateReturnsFalse() {
			var capId = Guid.NewGuid();
			_capAdmin.AddCap(ADMIN_TOKEN, capId);
			Assert.False(_capAdmin.AddCap(ADMIN_TOKEN, capId));
		}

		[Test]
		public void TestCapAdminAddCapUnlimitedDuplicateWrongTokenReturnsFalse() {
			var capId = Guid.NewGuid();
			_capAdmin.AddCap(ADMIN_TOKEN, capId);
			Assert.False(_capAdmin.AddCap("asdf", capId));
		}

		#endregion

		#region RemoveCap

		[Test]
		public void TestCapAdminRemoveCapDoesNotThrow() {
			var capId = Guid.NewGuid();
			_capAdmin.AddCap(ADMIN_TOKEN, capId);
			Assert.DoesNotThrow(() => _capAdmin.RemoveCap(ADMIN_TOKEN, capId));
		}

		[Test]
		public void TestCapAdminRemoveCapReturnsTrue() {
			var capId = Guid.NewGuid();
			_capAdmin.AddCap(ADMIN_TOKEN, capId);
			Assert.True(_capAdmin.RemoveCap(ADMIN_TOKEN, capId));
		}

		[Test]
		public void TestCapAdminRemoveCapWrongTokenReturnsFalse() {
			var capId = Guid.NewGuid();
			_capAdmin.AddCap(ADMIN_TOKEN, capId);
			Assert.False(_capAdmin.RemoveCap("asdf", capId));
		}


		[Test]
		public void TestCapAdminRemoveCapAndAddAgainDoesNotThrow() {
			var capId = Guid.NewGuid();
			_capAdmin.AddCap(ADMIN_TOKEN, capId);
			_capAdmin.RemoveCap(ADMIN_TOKEN, capId);
			Assert.DoesNotThrow(() => _capAdmin.AddCap(ADMIN_TOKEN, capId));
		}

		[Test]
		public void TestCapAdminRemoveCapAndAddAgainReturnsTrue() {
			var capId = Guid.NewGuid();
			_capAdmin.AddCap(ADMIN_TOKEN, capId);
			_capAdmin.RemoveCap(ADMIN_TOKEN, capId);
			Assert.True(_capAdmin.AddCap(ADMIN_TOKEN, capId));
		}

		[Test]
		public void TestCapAdminRemoveCapAndAddAgainWrongTokenReturnsFalse() {
			var capId = Guid.NewGuid();
			_capAdmin.AddCap(ADMIN_TOKEN, capId);
			_capAdmin.RemoveCap(ADMIN_TOKEN, capId);
			Assert.False(_capAdmin.AddCap("asdf", capId));
		}

		#endregion

		#region PauseCap

		[Test]
		public void TestCapAdminPauseCapDoesNotThrow() {
			var capId = Guid.NewGuid();
			_capAdmin.AddCap(ADMIN_TOKEN, capId);
			Assert.DoesNotThrow(() => _capAdmin.PauseCap(ADMIN_TOKEN, capId));
		}

		[Test]
		public void TestCapAdminPauseCapReturnsTrue() {
			var capId = Guid.NewGuid();
			_capAdmin.AddCap(ADMIN_TOKEN, capId);
			Assert.True(_capAdmin.PauseCap(ADMIN_TOKEN, capId));
		}

		[Test]
		public void TestCapAdminPauseCapWrongTokenReturnsFalse() {
			var capId = Guid.NewGuid();
			_capAdmin.AddCap(ADMIN_TOKEN, capId);
			Assert.False(_capAdmin.PauseCap("asdf", capId));
		}


		[Test]
		public void TestCapAdminPauseCapTwiceDoesNotThrow() {
			var capId = Guid.NewGuid();
			_capAdmin.AddCap(ADMIN_TOKEN, capId);
			_capAdmin.PauseCap(ADMIN_TOKEN, capId);
			Assert.DoesNotThrow(() => _capAdmin.PauseCap(ADMIN_TOKEN, capId));
		}

		[Test]
		public void TestCapAdminPauseCapTwiceReturnsTrue() {
			var capId = Guid.NewGuid();
			_capAdmin.AddCap(ADMIN_TOKEN, capId);
			_capAdmin.PauseCap(ADMIN_TOKEN, capId);
			Assert.True(_capAdmin.PauseCap(ADMIN_TOKEN, capId));
		}

		// TODO: tests to verify that it was actually paused...

		#endregion

		#region ResumeCap

		[Test]
		public void TestCapAdminResumeCapDoesNotThrow() {
			var capId = Guid.NewGuid();
			_capAdmin.AddCap(ADMIN_TOKEN, capId);
			_capAdmin.PauseCap(ADMIN_TOKEN, capId);
			Assert.DoesNotThrow(() => _capAdmin.ResumeCap(ADMIN_TOKEN, capId));
		}

		[Test]
		public void TestCapAdminResumeCapReturnsTrue() {
			var capId = Guid.NewGuid();
			_capAdmin.AddCap(ADMIN_TOKEN, capId);
			_capAdmin.PauseCap(ADMIN_TOKEN, capId);
			Assert.True(_capAdmin.ResumeCap(ADMIN_TOKEN, capId));
		}

		[Test]
		public void TestCapAdminResumeCapWrongTokenReturnsFalse() {
			var capId = Guid.NewGuid();
			_capAdmin.AddCap(ADMIN_TOKEN, capId);
			_capAdmin.PauseCap(ADMIN_TOKEN, capId);
			Assert.False(_capAdmin.ResumeCap("asdf", capId));
		}


		[Test]
		public void TestCapAdminResumeCapTwiceDoesNotThrow() {
			var capId = Guid.NewGuid();
			_capAdmin.AddCap(ADMIN_TOKEN, capId);
			_capAdmin.PauseCap(ADMIN_TOKEN, capId);
			_capAdmin.ResumeCap(ADMIN_TOKEN, capId);
			Assert.DoesNotThrow(() => _capAdmin.ResumeCap(ADMIN_TOKEN, capId));
		}

		[Test]
		public void TestCapAdminResumeCapTwiceReturnsTrue() {
			var capId = Guid.NewGuid();
			_capAdmin.AddCap(ADMIN_TOKEN, capId);
			_capAdmin.PauseCap(ADMIN_TOKEN, capId);
			_capAdmin.ResumeCap(ADMIN_TOKEN, capId);
			Assert.True(_capAdmin.ResumeCap(ADMIN_TOKEN, capId));
		}

		// TODO: tests to verify that it was actually resumed...

		#endregion

		#region LimitCap

		[Test]
		public void TestCapAdminLimitCapLLDoesNotThrow() {
			var capId = Guid.NewGuid();
			_capAdmin.AddCap(ADMIN_TOKEN, capId, 1000);
			Assert.DoesNotThrow(() => _capAdmin.LimitCap(ADMIN_TOKEN, capId, 100));
		}

		[Test]
		public void TestCapAdminLimitCapLLReturnsTrue() {
			var capId = Guid.NewGuid();
			_capAdmin.AddCap(ADMIN_TOKEN, capId, 1000);
			Assert.True(_capAdmin.LimitCap(ADMIN_TOKEN, capId, 100));
		}


		[Test]
		public void TestCapAdminLimitCapLUDoesNotThrow() {
			var capId = Guid.NewGuid();
			_capAdmin.AddCap(ADMIN_TOKEN, capId, 1000);
			Assert.DoesNotThrow(() => _capAdmin.LimitCap(ADMIN_TOKEN, capId));
		}

		[Test]
		public void TestCapAdminLimitCapLUReturnsTrue() {
			var capId = Guid.NewGuid();
			_capAdmin.AddCap(ADMIN_TOKEN, capId, 1000);
			Assert.True(_capAdmin.LimitCap(ADMIN_TOKEN, capId));
		}


		[Test]
		public void TestCapAdminLimitCapULDoesNotThrow() {
			var capId = Guid.NewGuid();
			_capAdmin.AddCap(ADMIN_TOKEN, capId, 1000);
			Assert.DoesNotThrow(() => _capAdmin.LimitCap(ADMIN_TOKEN, capId, 100));
		}

		[Test]
		public void TestCapAdminLimitCapULReturnsTrue() {
			var capId = Guid.NewGuid();
			_capAdmin.AddCap(ADMIN_TOKEN, capId);
			Assert.True(_capAdmin.LimitCap(ADMIN_TOKEN, capId, 100));
		}


		[Test]
		public void TestCapAdminLimitCapUUDoesNotThrow() {
			var capId = Guid.NewGuid();
			_capAdmin.AddCap(ADMIN_TOKEN, capId);
			Assert.DoesNotThrow(() => _capAdmin.LimitCap(ADMIN_TOKEN, capId));
		}

		[Test]
		public void TestCapAdminLimitCapUUReturnsTrue() {
			var capId = Guid.NewGuid();
			_capAdmin.AddCap(ADMIN_TOKEN, capId);
			Assert.True(_capAdmin.LimitCap(ADMIN_TOKEN, capId));
		}


		[Test]
		public void TestCapAdminLimitCapWrongTokenReturnsFalse() {
			var capId = Guid.NewGuid();
			_capAdmin.AddCap(ADMIN_TOKEN, capId);
			Assert.False(_capAdmin.LimitCap("asdf", capId));
		}

		// TODO: tests to verify that the limit was actually changed...

		#endregion
	}
}
