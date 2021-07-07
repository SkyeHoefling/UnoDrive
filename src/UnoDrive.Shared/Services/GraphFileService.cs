using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Graph;
using UnoDrive.Data;
using Windows.Networking.Connectivity;

namespace UnoDrive.Services
{
	public interface IGraphFileService
	{
		Task<IEnumerable<OneDriveItem>> GetRootFiles(Action<IEnumerable<OneDriveItem>, bool> cachedCallback = null, CancellationToken cancellationToken = default);
		Task<IEnumerable<OneDriveItem>> GetFiles(string id, Action<IEnumerable<OneDriveItem>, bool> cachedCallback = null, CancellationToken cancellationToken = default);
	}

	public class GraphFileService : IGraphFileService, IAuthenticationProvider
	{
#if DEBUG
		const int ApiDelayInMilliseconds = 500;
#endif

		readonly GraphServiceClient graphClient;
		readonly INetworkConnectivityService networkConnectivity;
		readonly CachedGraphFileService cachedService;
		public GraphFileService(INetworkConnectivityService networkConnectivity)
		{
			this.networkConnectivity = networkConnectivity;

#if __WASM__
            var httpClient = new HttpClient(new Uno.UI.Wasm.WasmHttpHandler());
#else
			var httpClient = new HttpClient();
#endif

			cachedService = new CachedGraphFileService();
			graphClient = new GraphServiceClient(httpClient);
			graphClient.AuthenticationProvider = this;
		}

		public async Task<IEnumerable<OneDriveItem>> GetRootFiles(Action<IEnumerable<OneDriveItem>, bool> cachedCallback = null, CancellationToken cancellationToken = default)
		{
			if (cachedCallback != null)
			{
				var rootId = await cachedService.GetRootId();
				if (!string.IsNullOrEmpty(rootId))
				{
					var cachedRootChildren = await cachedService.GetCachedFilesAsync(rootId);
					cachedCallback(cachedRootChildren, true);
				}
				else
					cachedCallback(new OneDriveItem[0], true);
			}

			// If the response is null that means we couldn't retrieve data
			// due to no internet connectivity
			if (networkConnectivity.Connectivity != NetworkConnectivityLevel.InternetAccess)
				return null;

			cancellationToken.ThrowIfCancellationRequested();

#if DEBUG
			await Task.Delay(ApiDelayInMilliseconds, cancellationToken);
#endif

			var rootChildren = (await graphClient.Me.Drive.Root.Children
				.Request()
				.GetAsync(cancellationToken))
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

			cancellationToken.ThrowIfCancellationRequested();

			if (!rootChildren.Any())
				return new OneDriveItem[0];

			await cachedService.SaveCachedFilesAsync(rootChildren);
			await cachedService.SaveRootIdAsync(rootChildren.FirstOrDefault().PathId);

			return rootChildren;
		}

		public async Task<IEnumerable<OneDriveItem>> GetFiles(string id, Action<IEnumerable<OneDriveItem>, bool> cachedCallback = null, CancellationToken cancellationToken = default)
		{
			if (cachedCallback != null)
			{
				var cachedChildren = await cachedService.GetCachedFilesAsync(id);
				cachedCallback(cachedChildren, true);
			}

			// If the response is null that means we couldn't retrieve data
			// due to no internet connectivity
			if (networkConnectivity.Connectivity != NetworkConnectivityLevel.InternetAccess)
				return null;

			cancellationToken.ThrowIfCancellationRequested();

#if DEBUG
			await Task.Delay(ApiDelayInMilliseconds, cancellationToken);
#endif

			var children = (await graphClient.Me.Drive.Items[id].Children
				.Request()
				.GetAsync(cancellationToken))
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

			cancellationToken.ThrowIfCancellationRequested();

			await cachedService.SaveCachedFilesAsync(children);
			return children;
		}

		Task IAuthenticationProvider.AuthenticateRequestAsync(HttpRequestMessage request)
		{
			var token = ((App)App.Current).AuthenticationResult?.AccessToken;
			if (string.IsNullOrEmpty(token))
				throw new System.Exception("No Access Token");

			request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
			return Task.CompletedTask;
		}

		class CachedGraphFileService
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

			public async Task SaveCachedFilesAsync(IEnumerable<OneDriveItem> children)
			{
				// TODO - ensure stale data is removed
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
		}
	}
}