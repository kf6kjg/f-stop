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
using InWorldz.Data.Assets.Stratus;
using LibF_Stop;

namespace f_stopUnitTests {
	[TestFixture]
	public class TestCapAdministration {
		private const string ADMIN_TOKEN = "test";

		private LibF_Stop.CapAdministration _capAdmin;
		private StratusAsset _knownMeshAsset;
		private StratusAsset _knownTextureAsset;

		[OneTimeSetUp]
		public void Setup() {
			TestCapability.CleanupCache();
			TestCapability.SetupCacheAndChattel();

			ConfigSingleton.ValidTypes.Add(49);

			// Using hardcoded GUIDs to make debugging easier.

			_knownMeshAsset = TestCapability.CreateAndCacheAsset(
				"_knownMeshAsset",
				49,
				new byte[] { 0xfa }, // TODO: find out what mesh's header is if any.
				Guid.Parse("05000000-0000-0000-0000-000000000000")
			);

			_knownTextureAsset = TestCapability.CreateAndCacheAsset(
				"_knownTextureAsset",
				0,
				new byte[] { 0x00, 0x00, 0x00, 0x0C, 0x6A, 0x50, 0x20, 0x20, 0x0D, 0x0A, 0x87, 0x0A }, // JPEG-2000 magic numbers
				Guid.Parse("01000000-0000-0000-0000-000000000000")
			);


			LibF_Stop.ConfigSingleton.AdminToken = ADMIN_TOKEN;
			_capAdmin = new LibF_Stop.CapAdministration();
		}

		[OneTimeTearDown]
		public void Cleanup() {
			TestCapability.CleanupCache();
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
		public void TestCapAdminAddCapLimitedWrongTokenThrowsInvalidAdminTokenException() {
			Assert.Throws<LibF_Stop.InvalidAdminTokenException>(() => _capAdmin.AddCap("asdf", Guid.NewGuid(), 100000));
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
		public void TestCapAdminAddCapUnlimitedWrongTokenThrowsInvalidAdminTokenException() {
			Assert.Throws<LibF_Stop.InvalidAdminTokenException>(() => _capAdmin.AddCap("asdf", Guid.NewGuid()));
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
		public void TestCapAdminAddCapLimitedDuplicateWrongTokenThrowsInvalidAdminTokenException() {
			var capId = Guid.NewGuid();
			_capAdmin.AddCap(ADMIN_TOKEN, capId, 100000);
			Assert.Throws<LibF_Stop.InvalidAdminTokenException>(() => _capAdmin.AddCap("asdf", capId, 100000));
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
		public void TestCapAdminAddCapUnlimitedDuplicateWrongTokenThrowsInvalidAdminTokenException() {
			var capId = Guid.NewGuid();
			_capAdmin.AddCap(ADMIN_TOKEN, capId);
			Assert.Throws<LibF_Stop.InvalidAdminTokenException>(() => _capAdmin.AddCap("asdf", capId));
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
		public void TestCapAdminRemoveCapWrongTokenThrowsInvalidAdminTokenException() {
			var capId = Guid.NewGuid();
			_capAdmin.AddCap(ADMIN_TOKEN, capId);
			Assert.Throws<LibF_Stop.InvalidAdminTokenException>(() => _capAdmin.RemoveCap("asdf", capId));
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
		public void TestCapAdminRemoveCapAndAddAgainWrongTokenThrowsInvalidAdminTokenException() {
			var capId = Guid.NewGuid();
			_capAdmin.AddCap(ADMIN_TOKEN, capId);
			_capAdmin.RemoveCap(ADMIN_TOKEN, capId);
			Assert.Throws<LibF_Stop.InvalidAdminTokenException>(() => _capAdmin.AddCap("asdf", capId));
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
		public void TestCapAdminPauseCapWrongTokenThrowsInvalidAdminTokenException() {
			var capId = Guid.NewGuid();
			_capAdmin.AddCap(ADMIN_TOKEN, capId);
			Assert.Throws<LibF_Stop.InvalidAdminTokenException>(() => _capAdmin.PauseCap("asdf", capId));
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

		[Test]
		public void TestCapAdminPauseCapRequestTimesOut() {
			var capId = Guid.NewGuid();
			_capAdmin.AddCap(ADMIN_TOKEN, capId);
			_capAdmin.PauseCap(ADMIN_TOKEN, capId);
			var gotAsset = false;
			var gotError = false;

			_capAdmin.RequestMeshAssetOnCap(capId, Guid.NewGuid(), asset => gotAsset = true, error => gotError = true);

			Assert.That(() => gotAsset || gotError, Is.False.After(100).MilliSeconds);
		}

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
		public void TestCapAdminResumeCapWrongTokenThrowsInvalidAdminTokenException() {
			var capId = Guid.NewGuid();
			_capAdmin.AddCap(ADMIN_TOKEN, capId);
			_capAdmin.PauseCap(ADMIN_TOKEN, capId);
			Assert.Throws<LibF_Stop.InvalidAdminTokenException>(() => _capAdmin.ResumeCap("asdf", capId));
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

		[Test]
		public void TestCapAdminResumeCapRequestTimesOut() {
			var capId = Guid.NewGuid();
			_capAdmin.AddCap(ADMIN_TOKEN, capId);
			_capAdmin.PauseCap(ADMIN_TOKEN, capId);
			_capAdmin.ResumeCap(ADMIN_TOKEN, capId);
			var gotAsset = false;
			var gotError = false;

			_capAdmin.RequestMeshAssetOnCap(capId, Guid.NewGuid(), asset => gotAsset = true, error => gotError = true);

			Assert.That(() => gotAsset || gotError, Is.True.After(100).MilliSeconds);
		}

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
		public void TestCapAdminLimitCapWrongTokenThrowsInvalidAdminTokenException() {
			var capId = Guid.NewGuid();
			_capAdmin.AddCap(ADMIN_TOKEN, capId);
			Assert.Throws<LibF_Stop.InvalidAdminTokenException>(() => _capAdmin.LimitCap("asdf", capId));
		}

		// TODO: tests to verify that the limit was actually changed...

		#endregion

		#region RequestMeshAssetOnCap

		[Test]
		public void TestCapAdmin_RequestMeshAssetOnCap_nullFailureHandler_ArgumentNullException() {
			var capId = Guid.NewGuid();
			_capAdmin.AddCap(ADMIN_TOKEN, capId);

			Assert.Throws<ArgumentNullException>(() => _capAdmin.RequestMeshAssetOnCap(capId, _knownMeshAsset.Id, a => { }, null));
		}

		[Test]
		public void TestCapAdmin_RequestMeshAssetOnCap_nullSuccessHandler_ArgumentNullException() {
			var capId = Guid.NewGuid();
			_capAdmin.AddCap(ADMIN_TOKEN, capId);

			Assert.Throws<ArgumentNullException>(() => _capAdmin.RequestMeshAssetOnCap(capId, _knownMeshAsset.Id, null, a => { }));
		}


		[Test]
		public void TestCapAdmin_RequestMeshAssetOnCap_badAssetId_CallsErrHandler() {
			var capId = Guid.NewGuid();
			_capAdmin.AddCap(ADMIN_TOKEN, capId);

			var gotCallback = false;
			_capAdmin.RequestMeshAssetOnCap(capId, Guid.NewGuid(), a => { }, a => gotCallback = true);

			Assert.That(() => gotCallback, Is.True.After(100).MilliSeconds);
		}

		[Test]
		public void TestCapAdmin_RequestMeshAssetOnCap_badAssetId_DoesntCallSuccessHandler() {
			var capId = Guid.NewGuid();
			_capAdmin.AddCap(ADMIN_TOKEN, capId);

			var gotCallback = false;
			_capAdmin.RequestMeshAssetOnCap(capId, Guid.NewGuid(), a => gotCallback = true, a => { });

			Assert.That(() => gotCallback, Is.False.After(100).MilliSeconds);
		}

		[Test]
		public void TestCapAdmin_RequestMeshAssetOnCap_badAssetId_AssetTypeWrong() {
			var capId = Guid.NewGuid();
			_capAdmin.AddCap(ADMIN_TOKEN, capId);

			var errorType = AssetErrorType.AssetTypeWrong;
			_capAdmin.RequestMeshAssetOnCap(capId, Guid.NewGuid(), a => { }, a => errorType = a.Error);

			Assert.That(() => errorType, Is.EqualTo(AssetErrorType.AssetIdUnknown).After(100).MilliSeconds);
		}


		[Test]
		public void TestCapAdmin_RequestMeshAssetOnCap_badAssetType_CallsErrHandler() {
			var capId = Guid.NewGuid();
			_capAdmin.AddCap(ADMIN_TOKEN, capId);

			var gotCallback = false;
			_capAdmin.RequestMeshAssetOnCap(capId, _knownTextureAsset.Id, a => { }, a => gotCallback = true);

			Assert.That(() => gotCallback, Is.True.After(100).MilliSeconds);
		}

		[Test]
		public void TestCapAdmin_RequestMeshAssetOnCap_badAssetType_DoesntCallSuccessHandler() {
			var capId = Guid.NewGuid();
			_capAdmin.AddCap(ADMIN_TOKEN, capId);

			var gotCallback = false;
			_capAdmin.RequestMeshAssetOnCap(capId, _knownTextureAsset.Id, a => gotCallback = true, a => { });

			Assert.That(() => gotCallback, Is.False.After(100).MilliSeconds);
		}

		[Test]
		public void TestCapAdmin_RequestMeshAssetOnCap_badAssetType_AssetTypeWrong() {
			var capId = Guid.NewGuid();
			_capAdmin.AddCap(ADMIN_TOKEN, capId);

			var errorType = AssetErrorType.AssetIdUnknown;
			_capAdmin.RequestMeshAssetOnCap(capId, _knownTextureAsset.Id, a => { }, a => errorType = a.Error);

			Assert.That(() => errorType, Is.EqualTo(AssetErrorType.AssetTypeWrong).After(100).MilliSeconds);
		}


		[Test]
		public void TestCapAdmin_RequestMeshAssetOnCap_badCap_CallsErrHandler() {
			var capId = Guid.NewGuid();
			_capAdmin.AddCap(ADMIN_TOKEN, capId);

			var gotCallback = false;
			_capAdmin.RequestMeshAssetOnCap(Guid.NewGuid(), _knownMeshAsset.Id, a => { }, a => gotCallback = true);

			Assert.That(() => gotCallback, Is.True.After(100).MilliSeconds);
		}

		[Test]
		public void TestCapAdmin_RequestMeshAssetOnCap_badCap_DoesntCallSuccessHandler() {
			var capId = Guid.NewGuid();
			_capAdmin.AddCap(ADMIN_TOKEN, capId);

			var gotCallback = false;
			_capAdmin.RequestMeshAssetOnCap(Guid.NewGuid(), _knownMeshAsset.Id, a => gotCallback = true, a => { });

			Assert.That(() => gotCallback, Is.False.After(100).MilliSeconds);
		}

		[Test]
		public void TestCapAdmin_RequestMeshAssetOnCap_badCap_CapabilityIdUnknown() {
			var capId = Guid.NewGuid();
			_capAdmin.AddCap(ADMIN_TOKEN, capId);

			var errorType = AssetErrorType.AssetIdUnknown;
			_capAdmin.RequestMeshAssetOnCap(Guid.NewGuid(), _knownMeshAsset.Id, a => { }, a => errorType = a.Error);

			Assert.That(() => errorType, Is.EqualTo(AssetErrorType.CapabilityIdUnknown).After(100).MilliSeconds);
		}


		[Test]
		public void TestCapAdmin_RequestMeshAssetOnCap_good_CallsSuccessHandler() {
			var capId = Guid.NewGuid();
			_capAdmin.AddCap(ADMIN_TOKEN, capId);

			var gotCallback = false;
			_capAdmin.RequestMeshAssetOnCap(capId, _knownMeshAsset.Id, a => gotCallback = true, a => { });

			Assert.That(() => gotCallback, Is.True.After(100).MilliSeconds);
		}

		[Test]
		public void TestCapAdmin_RequestMeshAssetOnCap_good_DoesntCallErrHandler() {
			var capId = Guid.NewGuid();
			_capAdmin.AddCap(ADMIN_TOKEN, capId);

			var errMessage = new AssetError();
			_capAdmin.RequestMeshAssetOnCap(capId, _knownMeshAsset.Id, a => { }, a => errMessage = a);

			Assert.That(() => errMessage, Is.EqualTo(new AssetError()).After(100).MilliSeconds, $"Got error:\n\n{errMessage}");
		}

		[Test]
		public void TestCapAdmin_RequestMeshAssetOnCap_good_CorrectAsset() {
			var capId = Guid.NewGuid();
			_capAdmin.AddCap(ADMIN_TOKEN, capId);

			StratusAsset asset = null;
			_capAdmin.RequestMeshAssetOnCap(capId, _knownMeshAsset.Id, a => asset = a, a => { });

			Assert.That(() => asset, Is.EqualTo(_knownMeshAsset).After(200).MilliSeconds);
		}

		#endregion

		#region RequestTextureAssetOnCap

		[Test]
		public void TestCapAdmin_RequestTextureAssetOnCap_nullFailureHandler_ArgumentNullException() {
			var capId = Guid.NewGuid();
			_capAdmin.AddCap(ADMIN_TOKEN, capId);

			Assert.Throws<ArgumentNullException>(() => _capAdmin.RequestTextureAssetOnCap(capId, _knownTextureAsset.Id, a => { }, null));
		}

		[Test]
		public void TestCapAdmin_RequestTextureAssetOnCap_nullSuccessHandler_ArgumentNullException() {
			var capId = Guid.NewGuid();
			_capAdmin.AddCap(ADMIN_TOKEN, capId);

			Assert.Throws<ArgumentNullException>(() => _capAdmin.RequestTextureAssetOnCap(capId, _knownTextureAsset.Id, null, a => { }));
		}


		[Test]
		public void TestCapAdmin_RequestTextureAssetOnCap_badAssetId_CallsErrHandler() {
			var capId = Guid.NewGuid();
			_capAdmin.AddCap(ADMIN_TOKEN, capId);

			var gotCallback = false;
			_capAdmin.RequestTextureAssetOnCap(capId, Guid.NewGuid(), a => { }, a => gotCallback = true);

			Assert.That(() => gotCallback, Is.True.After(100).MilliSeconds);
		}

		[Test]
		public void TestCapAdmin_RequestTextureAssetOnCap_badAssetId_DoesntCallSuccessHandler() {
			var capId = Guid.NewGuid();
			_capAdmin.AddCap(ADMIN_TOKEN, capId);

			var gotCallback = false;
			_capAdmin.RequestTextureAssetOnCap(capId, Guid.NewGuid(), a => gotCallback = true, a => { });

			Assert.That(() => gotCallback, Is.False.After(100).MilliSeconds);
		}

		[Test]
		public void TestCapAdmin_RequestTextureAssetOnCap_badAssetId_AssetTypeWrong() {
			var capId = Guid.NewGuid();
			_capAdmin.AddCap(ADMIN_TOKEN, capId);

			var errorType = AssetErrorType.AssetTypeWrong;
			_capAdmin.RequestTextureAssetOnCap(capId, Guid.NewGuid(), a => { }, a => errorType = a.Error);

			Assert.That(() => errorType, Is.EqualTo(AssetErrorType.AssetIdUnknown).After(100).MilliSeconds);
		}


		[Test]
		public void TestCapAdmin_RequestTextureAssetOnCap_badAssetType_CallsErrHandler() {
			var capId = Guid.NewGuid();
			_capAdmin.AddCap(ADMIN_TOKEN, capId);

			var gotCallback = false;
			_capAdmin.RequestTextureAssetOnCap(capId, _knownMeshAsset.Id, a => { }, a => gotCallback = true);

			Assert.That(() => gotCallback, Is.True.After(100).MilliSeconds);
		}

		[Test]
		public void TestCapAdmin_RequestTextureAssetOnCap_badAssetType_DoesntCallSuccessHandler() {
			var capId = Guid.NewGuid();
			_capAdmin.AddCap(ADMIN_TOKEN, capId);

			var gotCallback = false;
			_capAdmin.RequestTextureAssetOnCap(capId, _knownMeshAsset.Id, a => gotCallback = true, a => { });

			Assert.That(() => gotCallback, Is.False.After(100).MilliSeconds);
		}

		[Test]
		public void TestCapAdmin_RequestTextureAssetOnCap_badAssetType_AssetTypeWrong() {
			var capId = Guid.NewGuid();
			_capAdmin.AddCap(ADMIN_TOKEN, capId);

			var errorType = AssetErrorType.AssetIdUnknown;
			_capAdmin.RequestTextureAssetOnCap(capId, _knownMeshAsset.Id, a => { }, a => errorType = a.Error);

			Assert.That(() => errorType, Is.EqualTo(AssetErrorType.AssetTypeWrong).After(100).MilliSeconds);
		}


		[Test]
		public void TestCapAdmin_RequestTextureAssetOnCap_badCap_CallsErrHandler() {
			var capId = Guid.NewGuid();
			_capAdmin.AddCap(ADMIN_TOKEN, capId);

			var gotCallback = false;
			_capAdmin.RequestTextureAssetOnCap(Guid.NewGuid(), _knownTextureAsset.Id, a => { }, a => gotCallback = true);

			Assert.That(() => gotCallback, Is.True.After(100).MilliSeconds);
		}

		[Test]
		public void TestCapAdmin_RequestTextureAssetOnCap_badCap_DoesntCallSuccessHandler() {
			var capId = Guid.NewGuid();
			_capAdmin.AddCap(ADMIN_TOKEN, capId);

			var gotCallback = false;
			_capAdmin.RequestTextureAssetOnCap(Guid.NewGuid(), _knownTextureAsset.Id, a => gotCallback = true, a => { });

			Assert.That(() => gotCallback, Is.False.After(100).MilliSeconds);
		}

		[Test]
		public void TestCapAdmin_RequestTextureAssetOnCap_badCap_CapabilityIdUnknown() {
			var capId = Guid.NewGuid();
			_capAdmin.AddCap(ADMIN_TOKEN, capId);

			var errorType = AssetErrorType.AssetIdUnknown;
			_capAdmin.RequestTextureAssetOnCap(Guid.NewGuid(), _knownTextureAsset.Id, a => { }, a => errorType = a.Error);

			Assert.That(() => errorType, Is.EqualTo(AssetErrorType.CapabilityIdUnknown).After(100).MilliSeconds);
		}


		[Test]
		public void TestCapAdmin_RequestTextureAssetOnCap_good_CallsSuccessHandler() {
			var capId = Guid.NewGuid();
			_capAdmin.AddCap(ADMIN_TOKEN, capId);

			var gotCallback = false;
			_capAdmin.RequestTextureAssetOnCap(capId, _knownTextureAsset.Id, a => gotCallback = true, a => { });

			Assert.That(() => gotCallback, Is.True.After(100).MilliSeconds);
		}

		[Test]
		public void TestCapAdmin_RequestTextureAssetOnCap_good_DoesntCallErrHandler() {
			var capId = Guid.NewGuid();
			_capAdmin.AddCap(ADMIN_TOKEN, capId);

			var errMessage = new AssetError();
			_capAdmin.RequestTextureAssetOnCap(capId, _knownTextureAsset.Id, a => { }, a => errMessage = a);

			Assert.That(() => errMessage, Is.EqualTo(new AssetError()).After(100).MilliSeconds, $"Got error:\n\n{errMessage}");
		}

		[Test]
		public void TestCapAdmin_RequestTextureAssetOnCap_good_CorrectAsset() {
			var capId = Guid.NewGuid();
			_capAdmin.AddCap(ADMIN_TOKEN, capId);

			StratusAsset asset = null;
			_capAdmin.RequestTextureAssetOnCap(capId, _knownTextureAsset.Id, a => asset = a, a => { });

			Assert.That(() => asset, Is.EqualTo(_knownTextureAsset).After(200).MilliSeconds);
		}

		#endregion
	}
}