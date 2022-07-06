using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using UnoDrive.Data;

namespace UnoDrive.Services
{
	public interface ICachedGraphFileService
	{
		Task SaveRootIdAsync(string rootId);
		Task<string> GetRootId();
		Task<IEnumerable<OneDriveItem>> GetCachedFilesAsync(string pathId);
		Task SaveCachedFilesAsync(IEnumerable<OneDriveItem> children, string pathId);
	}

	public class CachedGraphFileService : ICachedGraphFileService
	{
		public async Task SaveRootIdAsync(string rootId)
		{
			using (var dbContext = new UnoDriveDbContext())
			{
				var findRootIdSetting = await dbContext.Settings.FindAsync("RootId");
				if (findRootIdSetting != null)
				{
					findRootIdSetting.Value = rootId;
					dbContext.Settings.Update(findRootIdSetting);
				}
				else
				{
					var setting = new Setting { Key = "RootId", Value = rootId };
					await dbContext.Settings.AddAsync(setting);
				}

				await dbContext.SaveChangesAsync();
			}
		}

		public async Task<string> GetRootId()
		{
			using (var dbContext = new UnoDriveDbContext())
			{
				var rootId = await dbContext.Settings.FindAsync("RootId");
				return rootId != null ? rootId.Value : string.Empty;
			}
		}

		public async Task<IEnumerable<OneDriveItem>> GetCachedFilesAsync(string pathId)
		{
			if (string.IsNullOrEmpty(pathId))
				return new OneDriveItem[0];

			using (var dbContext = new UnoDriveDbContext())
			{
				return await dbContext.OneDriveItems
					.AsNoTracking()
					.Where(item => item.PathId == pathId)
					.OrderByDescending(item => item.Type)
					.ThenBy(item => item.Name)
					.ToArrayAsync();
			}
		}

		public async Task SaveCachedFilesAsync(IEnumerable<OneDriveItem> children, string pathId)
		{
			using (var dbContext = new UnoDriveDbContext())
			{
				var childrenIds = children.Select(c => c.Id).ToArray();
				var staleItems = await dbContext.OneDriveItems
					.Where(i => i.PathId == pathId && !childrenIds.Any(c => c == i.Id))
					.ToArrayAsync();

				if (staleItems != null && staleItems.Any())
				{
					dbContext.OneDriveItems.RemoveRange(staleItems);
					foreach (var item in staleItems.Where(i => !string.IsNullOrEmpty(i.ThumbnailPath)))
					{
						if (System.IO.File.Exists(item.ThumbnailPath))
							System.IO.File.Delete(item.ThumbnailPath);
					}
				}

				foreach (var item in children)
				{
					var findItem = await dbContext.OneDriveItems.AsNoTracking()
						.FirstOrDefaultAsync(i => i.Id == item.Id);
					if (findItem != null)
						dbContext.OneDriveItems.Update(item);
					else
						await dbContext.OneDriveItems.AddAsync(item);
				}

				await dbContext.SaveChangesAsync();
			}
		}
	}
}
