using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using LiteDB;
using UnoDrive.Data;

namespace UnoDrive.Services
{
	public class CachedGraphFileService : ICachedGraphFileService
	{
		string databaseName;
		public CachedGraphFileService()
		{
#if HAS_UNO_SKIA_WPF
			var applicationFolder = Path.Combine(Windows.Storage.ApplicationData.Current.TemporaryFolder.Path, "UnoDrive");
			databaseName = Path.Combine(applicationFolder, "UnoDriveData.db");
#else
			databaseName = Path.Combine(Windows.Storage.ApplicationData.Current.LocalFolder.Path, "UnoDriveData.db");
#endif
		}

		public void SaveRootId(string rootId)
		{
			using (var db = new LiteDatabase(databaseName))
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
					var newSetting = new Setting { Key = "RootId", Value = rootId };
					settings.Insert(newSetting);
				}
			}
		}

		public string GetRootId()
		{
			using (var db = new LiteDatabase(databaseName))
			{
				var settings = db.GetCollection<Setting>();
				var rootId = settings.FindById("RootId");
				return rootId != null ? rootId.Value : string.Empty;
			}
		}

		public IEnumerable<OneDriveItem> GetCachedFiles(string pathId)
		{
			// TODO - instantiate ImageSource for cached data
			if (string.IsNullOrEmpty(pathId))
				return new OneDriveItem[0];

			using (var db = new LiteDatabase(databaseName))
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
			using (var db = new LiteDatabase(databaseName))
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
			using (var db = new LiteDatabase(databaseName))
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
