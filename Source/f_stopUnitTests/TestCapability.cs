// TestCapability.cs
//
// Author:
//       Ricky Curtice <ricky@rwcproductions.com>
//
// Copyright (c) 2018 Richard Curtice
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
using InWorldz.Data.Assets.Stratus;
using LibF_Stop;
using NUnit.Framework;

namespace f_stopUnitTests {
	[TestFixture]
	public class TestCapability {
		private StratusAsset _knownTextureAsset;

		public static StratusAsset CreateAndCacheAsset(string name, sbyte type, byte[] data, Guid? id = null) {
			var asset = new StratusAsset {
				CreateTime = DateTime.UtcNow,
				Data = data,
				Description = $"{name} description",
				Id = id ?? Guid.NewGuid(),
				Local = true,
				Name = name,
				StorageFlags = 0,
				Temporary = false,
				Type = type,
			};

			var assetPath = UuidToCachePath(asset.Id);

			Directory.CreateDirectory(Directory.GetParent(assetPath).FullName);
			using (var file = File.Create(assetPath)) {
				ProtoBuf.Serializer.Serialize(file, asset);
			}

			return asset;
		}

		/// <summary>
		/// Converts a GUID to a path based on the cache location.
		/// </summary>
		/// <returns>The path.</returns>
		/// <param name="id">Asset identifier.</param>
		private static string UuidToCachePath(Guid id) {
			var noPunctuationAssetId = id.ToString("N");
			var path = Constants.TEST_CACHE_PATH;
			for (var index = 0; index < noPunctuationAssetId.Length; index += 2) {
				path = Path.Combine(path, noPunctuationAssetId.Substring(index, 2));
			}
			return path + ".pbasset";
		}

		public static void SetupCacheAndChattel() {
			Directory.CreateDirectory(Constants.TEST_CACHE_PATH);

			ConfigSingleton.ChattelReader = new Chattel.ChattelReader(
				new Chattel.ChattelConfiguration(Constants.TEST_CACHE_PATH, Constants.TEST_WRITE_CACHE_PATH)
			);
			ConfigSingleton.NegativeCacheItemLifetime = Constants.SERVICE_NC_LIFETIME;
			ConfigSingleton.ValidTypes = new System.Collections.Concurrent.ConcurrentBag<sbyte> {
				0
			};
		}

		public static void CleanupCache() {
			try {
				Directory.Delete(Constants.TEST_CACHE_PATH, true);
			}
			catch (DirectoryNotFoundException) {
			}
			try {
				File.Delete(Constants.TEST_WRITE_CACHE_PATH);
			}
			catch (FileNotFoundException) {
			}
		}

		[OneTimeSetUp]
		public void Setup() {
			CleanupCache();
			SetupCacheAndChattel();

			// Using hardcoded GUIDs to make debugging easier.

			_knownTextureAsset = CreateAndCacheAsset(
				"_knownTextureAsset",
				0,
				new byte[] { 0x00, 0x00, 0x00, 0x0C, 0x6A, 0x50, 0x20, 0x20, 0x0D, 0x0A, 0x87, 0x0A }, // JPEG-2000 magic numbers
				Guid.Parse("01000000-0000-0000-0000-000000000000")
			);
		}

		[OneTimeTearDown]
		public void Cleanup() {
			CleanupCache();
		}

		#region Pause Resume

		[Test]
		public void TestCapability_Pause_SetsIsActive_False() {
			var cap = new Capability();
			cap.Pause();
			Assert.False(cap.IsActive);
		}

		[Test]
		public void TestCapability_Resume_SetsIsActive_True() {
			var cap = new Capability();
			cap.Pause();
			cap.Resume();
			Assert.True(cap.IsActive);
		}

		#endregion

		#region Purge

		[Test]
		public void TestCapability_PurgeAndKill_SetsIsActive_False() {
			var cap = new Capability();
			cap.PurgeAndKill();
			Assert.False(cap.IsActive);
		}

		[Test]
		public void TestCapability_PurgeAndKill_FulfilledKnownIdRequest() {
			var cap = new Capability();
			var gotCallback = false;
			cap.Pause();
			cap.RequestAsset(_knownTextureAsset.Id, asset => gotCallback = true, error => gotCallback = true);

			cap.PurgeAndKill();

			Assert.That(() => gotCallback, Is.True.After(100).MilliSeconds);
		}

		[Test]
		public void TestCapability_PurgeAndKill_FulfilledUnknownIdRequest() {
			var cap = new Capability();
			var gotCallback = false;
			cap.Pause();
			cap.RequestAsset(Guid.NewGuid(), asset => gotCallback = true, error => gotCallback = true);

			cap.PurgeAndKill();

			Assert.That(() => gotCallback, Is.True.After(100).MilliSeconds);
		}

		#endregion

		#region RequestAsset

		[Test]
		public void TestCapability_RequestAsset_KnownId_CorrectAsset() {
			var cap = new Capability();
			StratusAsset assetReturned = null;
			cap.RequestAsset(_knownTextureAsset.Id, asset => assetReturned = asset, error => { });

			Assert.That(() => assetReturned, Is.EqualTo(_knownTextureAsset).After(200).MilliSeconds);
		}

		[Test]
		public void TestCapability_RequestAsset_KnownId_FailureHandlerNotCalled() {
			var cap = new Capability();
			var callbackError = new AssetError();
			cap.RequestAsset(_knownTextureAsset.Id, asset => { }, error => callbackError = error);

			Assert.That(() => callbackError, Is.EqualTo(new AssetError()).After(200).MilliSeconds, $"Got error:\n\n{callbackError}");
		}

		[Test]
		public void TestCapability_RequestAsset_KnownId_SuccessHandlerCalled() {
			var cap = new Capability();
			var gotCallback = false;
			cap.RequestAsset(_knownTextureAsset.Id, asset => gotCallback = true, error => {});

			Assert.That(() => gotCallback, Is.True.After(200).MilliSeconds);
		}

		[Test]
		public void TestCapability_RequestAsset_RandomId_FailureHandlerCalled() {
			var cap = new Capability();
			var gotCallback = false;
			cap.RequestAsset(Guid.NewGuid(), asset => { }, error => gotCallback = true);

			Assert.That(() => gotCallback, Is.True.After(200).MilliSeconds);
		}

		[Test]
		public void TestCapability_RequestAsset_RandomId_SuccessHandlerNotCalled() {
			var cap = new Capability();
			var gotCallback = false;
			cap.RequestAsset(Guid.NewGuid(), asset => gotCallback = true, error => {});

			Assert.That(() => gotCallback, Is.False.After(200).MilliSeconds);
		}

		[Test]
		public void TestCapability_RequestAsset_PausedNeverAnswers() {
			var cap = new Capability();
			cap.Pause();

			var gotCallback = false;
			cap.RequestAsset(Guid.NewGuid(), asset => gotCallback = true, error => gotCallback = true);

			Assert.That(() => gotCallback, Is.False.After(200).MilliSeconds);
		}

		#endregion

		// TODO: Test bandwidth stuff once figured out.
	}
}
