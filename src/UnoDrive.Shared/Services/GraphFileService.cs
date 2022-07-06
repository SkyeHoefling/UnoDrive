using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Graph;
using UnoDrive.Data;
using Xamarin.Essentials;

namespace UnoDrive.Services
{
	public interface IGraphFileService
	{
		Task<IEnumerable<OneDriveItem>> GetRootFiles(Action<IEnumerable<OneDriveItem>> cachedCallback = null);
		Task<IEnumerable<OneDriveItem>> GetFiles(string id, Action<IEnumerable<OneDriveItem>> cachedCallback = null);
	}

	public class GraphFileService : IGraphFileService, IAuthenticationProvider
	{
		readonly GraphServiceClient graphClient;
		public GraphFileService()
		{
#if __WASM__
            var httpClient = new HttpClient(new Uno.UI.Wasm.WasmHttpHandler());
#else
			var httpClient = new HttpClient();
#endif

			graphClient = new GraphServiceClient(httpClient);
			graphClient.AuthenticationProvider = this;
		}

		public async Task<IEnumerable<OneDriveItem>> GetRootFiles(Action<IEnumerable<OneDriveItem>> cachedCallback = null)
		{
			if (cachedCallback != null)
			{
				var rootId = await GetRootId();
				if (!string.IsNullOrEmpty(rootId))
				{
					var cachedRootChildren = await GetCachedFilesAsync(rootId);
					cachedCallback(cachedRootChildren);
				}
			}

			// If the response is null that means we couldn't retrieve data
			// due to no internet connectivity
			if (Connectivity.NetworkAccess != NetworkAccess.Internet)
				return null;

			var rootChildren = (await graphClient.Me.Drive.Root.Children
				.Request()
				.GetAsync())
				.Select(driveItem => new OneDriveItem
				{
					Id = driveItem.Id,
					Name = driveItem.Name,
					Path = driveItem.ParentReference.Path,
					PathId = driveItem.ParentReference.Id,
					FileSize = $"{driveItem.Size}",
					Modified = driveItem.LastModifiedDateTime.HasValue ?
						driveItem.LastModifiedDateTime.Value.LocalDateTime : DateTime.Now,
					Type = driveItem.Folder != null ? OneDriveItemType.Folder : OneDriveItemType.File
					//ModifiedBy = driveItem.LastModifiedByUser.DisplayName,
					//Sharing = ""
				})
				.OrderByDescending(item => item.Type)
				.ThenBy(item => item.Name);

			if (!rootChildren.Any())
				return new OneDriveItem[0];

			await SaveCachedFilesAsync(rootChildren);
			using (var dbContext = new UnoDriveDbContext())
			{
				var findRootIdSetting = await dbContext.Settings.FindAsync("RootId");
				if (findRootIdSetting != null)
				{
					findRootIdSetting.Value = rootChildren.FirstOrDefault().PathId;
					dbContext.Settings.Update(findRootIdSetting);
				}
				else
				{
					var setting = new Setting {Key = "RootId", Value = rootChildren.FirstOrDefault().PathId};
					await dbContext.Settings.AddAsync(setting);
				}

				await dbContext.SaveChangesAsync();
			}

			return rootChildren;
		}

		public async Task<IEnumerable<OneDriveItem>> GetFiles(string id, Action<IEnumerable<OneDriveItem>> cachedCallback = null)
		{
			if (cachedCallback != null)
			{
				var cachedChildren = await GetCachedFilesAsync(id);
				cachedCallback(cachedChildren);
			}

			// If the response is null that means we couldn't retrieve data
			// due to no internet connectivity
			if (Connectivity.NetworkAccess != NetworkAccess.Internet)
				return null;

			var children = (await graphClient.Me.Drive.Items[id].Children
				.Request()
				.GetAsync())
				.Select(driveItem => new OneDriveItem
				{
					Id = driveItem.Id,
					Name = driveItem.Name,
					Path = driveItem.ParentReference.Path,
					PathId = driveItem.ParentReference.Id,
					FileSize = $"{driveItem.Size}",
					Modified = driveItem.LastModifiedDateTime.HasValue ?
						driveItem.LastModifiedDateTime.Value.LocalDateTime : DateTime.Now,
					Type = driveItem.Folder != null ? OneDriveItemType.Folder : OneDriveItemType.File
					//ModifiedBy = driveItem.LastModifiedByUser.DisplayName,
					//Sharing = ""
				})
				.OrderByDescending(item => item.Type)
				.ThenBy(item => item.Name);

			await SaveCachedFilesAsync(children);
			return children;
		}

		async Task<string> GetRootId()
		{
			using (var dbContext = new UnoDriveDbContext())
			{
				var rootId = await dbContext.Settings.FindAsync("RootId");
				return rootId != null ? rootId.Value : string.Empty;
			}
		}

		async Task<IEnumerable<OneDriveItem>> GetCachedFilesAsync(string pathId)
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

		async Task SaveCachedFilesAsync(IEnumerable<OneDriveItem> children)
		{
			if (!children.Any())
				return;

			using (var dbContext = new UnoDriveDbContext())
			{
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

		Task IAuthenticationProvider.AuthenticateRequestAsync(HttpRequestMessage request)
		{
			var token = ((App)App.Current).AuthenticationResult?.AccessToken;
			if (string.IsNullOrEmpty(token))
				throw new System.Exception("No Access Token");

			request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
			return Task.CompletedTask;
		}
	}
}