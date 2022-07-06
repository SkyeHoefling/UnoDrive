using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Graph;
using UnoDrive.Data;
using Windows.Networking.Connectivity;

namespace UnoDrive.Services
{
	public interface IGraphFileService
	{
		Task<IEnumerable<OneDriveItem>> GetRootFiles(Action<IEnumerable<OneDriveItem>, bool> cachedCallback = null, CancellationToken cancellationToken = default);
		Task<IEnumerable<OneDriveItem>> GetFiles(string id, Action<IEnumerable<OneDriveItem>, bool> cachedCallback = null, CancellationToken cancellationToken = default);
		Task<Stream> GetThumbnailAsync(string id, CancellationToken cancellationToken);
	}

	public class GraphFileService : IGraphFileService, IAuthenticationProvider
	{
#if DEBUG
		const int ApiDelayInMilliseconds = 5000;
#endif

		readonly GraphServiceClient graphClient;
		readonly INetworkConnectivityService networkConnectivity;
		readonly ILogger logger;
		readonly CachedGraphFileService cachedService;
		public GraphFileService(INetworkConnectivityService networkConnectivity, ILogger<GraphFileService> logger)
		{
			this.networkConnectivity = networkConnectivity;
			this.logger = logger;

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

			var oneDriveItems = (await graphClient.Me.Drive.Items[id].Children
				.Request()
				.Expand("thumbnails")
				.GetAsync(cancellationToken))
				.ToArray();

			var childrenTable = oneDriveItems
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
				.ThenBy(item => item.Name)
				.ToDictionary(x => x.Id);

			cancellationToken.ThrowIfCancellationRequested();

			if (cachedCallback != null)
				cachedCallback(childrenTable.Select(x => x.Value), false);

			var children = childrenTable.Select(x => x.Value).ToArray();
			await cachedService.SaveCachedFilesAsync(children);

			for (int index = 0; index < oneDriveItems.Length; index++)
			{
				var currentItem = oneDriveItems[index];
				var thumbnails = currentItem.Thumbnails?.FirstOrDefault();
				if (thumbnails != null && !childrenTable.ContainsKey(currentItem.Id))
					continue;

				var url = thumbnails.Medium.Url;

				var client = new HttpClient();
				var response = await client.GetAsync(url);
				if (!response.IsSuccessStatusCode)
					continue;

				var imagesFolder = Path.Combine(Windows.Storage.ApplicationData.Current.LocalFolder.Path, "thumbnails");
				var name = $"{currentItem.Id}.jpeg";
				var localFilePath = Path.Combine(imagesFolder, name);

				try
				{
					if (!System.IO.Directory.Exists(imagesFolder))
						System.IO.Directory.CreateDirectory(imagesFolder);

					if (System.IO.File.Exists(localFilePath))
						System.IO.File.Delete(localFilePath);

					await System.IO.File.WriteAllBytesAsync(localFilePath, await response.Content.ReadAsByteArrayAsync(), cancellationToken);
					childrenTable[currentItem.Id].ThumbnailPath = localFilePath;

					if (cachedCallback != null)
						cachedCallback(childrenTable.Select(x => x.Value), false);

					using (var dbContext = new UnoDriveDbContext())
					{
						var findCachedFile = await dbContext.OneDriveItems.FindAsync(currentItem.Id);
						if (findCachedFile != null)
						{
							findCachedFile.ThumbnailPath = localFilePath;
							await dbContext.SaveChangesAsync();
						}
					}

					cancellationToken.ThrowIfCancellationRequested();
				}
				catch (TaskCanceledException ex)
				{
					logger.LogWarning(ex, ex.Message);
					throw ex;
				}
				catch (Exception ex)
				{
					logger.LogError(ex, ex.Message);
				}
			}

			return children;
		}

		public Task<Stream> GetThumbnailAsync(string id, CancellationToken cancellationToken)
		{
			throw new NotImplementedException();
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