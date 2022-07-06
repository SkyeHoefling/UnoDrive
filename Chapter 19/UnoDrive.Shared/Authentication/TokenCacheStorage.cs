﻿using System;
using System.IO;
using LiteDB;
using Microsoft.Identity.Client;
using Windows.Storage;

namespace UnoDrive.Authentication
{
	static class TokenCacheStorage
	{
		static string GetConnectionString()
		{
#if HAS_UNO_SKIA_WPF
			var applicationFolder = Path.Combine(ApplicationData.Current.TemporaryFolder.Path, "UnoDrive");
			if (!Directory.Exists(applicationFolder))
			{
				Directory.CreateDirectory(applicationFolder);
			}

			var databaseFile = Path.Combine(applicationFolder, "UnoDrive_MSAL_TokenCache.db");
#else
			var databaseFile = Path.Combine(ApplicationData.Current.LocalFolder.Path, "UnoDrive_MSAL_TokenCache.db");
#endif

			return $"Filename={databaseFile}";
		}

		public static void EnableSerialization(ITokenCache tokenCache)
		{
			tokenCache.SetBeforeAccess(BeforeAccessNotification);
			tokenCache.SetAfterAccess(AfterAccessNotification);
		}

		static void BeforeAccessNotification(TokenCacheNotificationArgs args)
		{
			using (var db = new LiteDatabase(GetConnectionString()))
			{
				var tokens = db.GetCollection<TokenCache>();
				var tokenCache = tokens.Query().FirstOrDefault();
				var serializedCache = tokenCache != null ?
					Convert.FromBase64String(tokenCache.Data) : null;

				args.TokenCache.DeserializeMsalV3(serializedCache);
			}
		}

		static void AfterAccessNotification(TokenCacheNotificationArgs args)
		{
			var data = args.TokenCache.SerializeMsalV3();
			var serializedCache = Convert.ToBase64String(data);

			using (var db = new LiteDatabase(GetConnectionString()))
			{
				var tokens = db.GetCollection<TokenCache>();
				tokens.DeleteAll();
				tokens.Insert(new TokenCache { Data = serializedCache });
			}
		}

		class TokenCache
		{
			public string Data { get; set; }
		}
	}
}