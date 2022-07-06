using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using LiteDB;
using Uno.Extensions;
using UnoDrive.Data;
using Windows.Storage;

namespace UnoDrive.Data
{
	public class DataStore : IDataStore
	{
		readonly string databaseFile;
		public DataStore()
		{
#if HAS_UNO_SKIA_WPF
			var applicationFolder = Path.Combine(ApplicationData.Current.TemporaryFolder.Path, "UnoDrive");
			databaseFile = Path.Combine(applicationFolder, "UnoDriveData.db");
#else
			databaseFile = Path.Combine(ApplicationData.Current.LocalFolder.Path, "UnoDriveData.db");
#endif
		}

		public void SaveUserInfo(UserInfo userInfo)
		{
			using (var db = new LiteDatabase(databaseFile))
			{
				var users = db.GetCollection<UserInfo>();
				var findUserInfo = users.FindById(userInfo.Id);

				if (findUserInfo != null)
				{
					findUserInfo.Name = userInfo.Name;
					findUserInfo.Email = userInfo.Email;

					users.Update(findUserInfo);
				}
				else
				{
					users.Insert(userInfo);
				}
			}
		}

		public UserInfo GetUserInfoById(string userId)
		{
			using (var db = new LiteDatabase(databaseFile))
			{
				var users = db.GetCollection<UserInfo>();
				return users.FindById(userId);
			}
		}

		public void SaveRootId(string rootId)
		{
			using (var db = new LiteDatabase(databaseFile))
			{
				var settings = db.GetCollection<Setting>();
				var findRootIdSetting = settings.FindById("RootId");

				if (findRootIdSetting != null)
				{
					findRootIdSetting.Value = rootId;
					settings.Update(findRootIdSetting);
				}
				else
				{
					var newSetting = new Setting { Id = "RootId", Value = rootId };
					settings.Insert(newSetting);
				}
			}
		}

		public string GetRootId()
		{
			using (var db = new LiteDatabase(databaseFile))
			{
				var settings = db.GetCollection<Setting>();
				var rootId = settings.FindById("RootId");
				return rootId != null ? rootId.Value : string.Empty;
			}
		}

		public IEnumerable<OneDriveItem> GetCachedFiles(string pathId)
		{
			if (string.IsNullOrEmpty(pathId))
				return new OneDriveItem[0];

			using (var db = new LiteDatabase(databaseFile))
			{
				var items = db.GetCollection<OneDriveItem>();
				return items
					.Query()
					.Where(item => item.PathId == pathId)
					.ToArray();
			}
		}

		public void SaveCachedFiles(IEnumerable<OneDriveItem> children, string pathId)
		{
			using (var db = new LiteDatabase(databaseFile))
			{
				var childrenIds = children.Select(c => c.Id).ToArray();
				var items = db.GetCollection<OneDriveItem>();
				
				var staleItems = items
					.Query()
					.Where(i => i.PathId == pathId && !childrenIds.Any(c => c == i.Id))
					.ToArray();

				if (staleItems != null && staleItems.Any())
				{
					items.DeleteMany(x => staleItems.Contains(x));
					foreach (var item in staleItems.Where(i => !string.IsNullOrEmpty(i.ThumbnailPath)))
					{
						if (System.IO.File.Exists(item.ThumbnailPath))
							System.IO.File.Delete(item.ThumbnailPath);
					}
				}

				foreach (var item in children)
				{
					var findItem = items.FindById(item.Id);
					if (findItem != null)
					{
						items.Update(item);
					}
					else
					{
						items.Insert(item);
					}
				}
			}
		}

		public void UpdateCachedFileById(string itemId, string localFilePath)
		{
			using (var db = new LiteDatabase(databaseFile))
			{
				var items = db.GetCollection<OneDriveItem>();
				var findItem = items.FindById(itemId);
				if (findItem != null)
				{
					findItem.ThumbnailPath = localFilePath;
					items.Update(findItem);
				}
			}
		}
	}
}
