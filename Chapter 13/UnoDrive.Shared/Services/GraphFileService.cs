using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.Graph;
using Microsoft.UI.Xaml.Media.Imaging;
using Newtonsoft.Json;
using UnoDrive.Data;

namespace UnoDrive.Services
{
	public class GraphFileService : IGraphFileService, IAuthenticationProvider
    {
#if DEBUG
		const int ApiDelayInMilliseconds = 5000;
#endif

		GraphServiceClient graphClient;
		ILogger logger;

		public GraphFileService(ILogger<GraphFileService> logger)
		{
			this.logger = logger;

#if __WASM__
			var httpClient = new HttpClient(new Uno.UI.Wasm.WasmHttpHandler());
#else
			var httpClient = new HttpClient();
#endif

			graphClient = new GraphServiceClient(httpClient);
			graphClient.AuthenticationProvider = this;
		}

		public async Task<IEnumerable<OneDriveItem>> GetRootFilesAsync()
		{
			var rootPathId = string.Empty;

			try
			{
				var request = graphClient.Me.Drive.Root.Request();

#if __ANDROID__
				var response = await request.GetResponseAsync();
				var data = await response.Content.ReadAsStringAsync();
				var rootNode = JsonConvert.DeserializeObject<DriveItem>(data);
#else
				var rootNode = await request.GetAsync();
#endif

				if (rootNode == null || string.IsNullOrEmpty(rootNode.Id))
				{
					throw new KeyNotFoundException("Unable to find OneDrive Root Folder");
				}

				rootPathId = rootNode.Id;
			}
			catch (KeyNotFoundException ex)
			{
				logger.LogWarning("Unable to retrieve data from Graph API, it may not exist or there could be a connection issue");
				logger.LogWarning(ex, ex.Message);
				throw ex;
			}
			catch (Exception ex)
			{
				logger.LogWarning("Unable to retrieve root OneDrive folder, attempting to use local data instead");
				logger.LogWarning(ex, ex.Message);
			}

			return await GetFilesAsync(rootPathId);
		}

		public async Task<IEnumerable<OneDriveItem>> GetFilesAsync(string id)
		{
#if DEBUG
			await Task.Delay(ApiDelayInMilliseconds);
#endif

			var request = graphClient.Me.Drive.Items[id].Children
				.Request()
				.Expand("thumbnails");

#if __ANDROID__
			var response = await request.GetResponseAsync();
			var data = await response.Content.ReadAsStringAsync();
			var collection = JsonConvert.DeserializeObject<UnoDrive.Models.DriveItemCollection>(data);
			var oneDriveItems = collection.Value;
#else
			var oneDriveItems = (await request.GetAsync()).ToArray();
#endif

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
				})
				.OrderByDescending(item => item.Type)
				.ThenBy(item => item.Name)
				.ToDictionary(x => x.Id);

			var children = childrenTable.Select(x => x.Value).ToArray();

			for (int index = 0; index < oneDriveItems.Length; index++)
			{
				var currentItem = oneDriveItems[index];
				var thumbnails = currentItem.Thumbnails?.FirstOrDefault();
				if (thumbnails == null || !childrenTable.ContainsKey(currentItem.Id))
					continue;

				var url = thumbnails.Medium.Url;

#if __WASM__
				var httpClient = new HttpClient(new Uno.UI.Wasm.WasmHttpHandler());
#else
				var httpClient = new HttpClient();
#endif
				var thumbnailResponse = await httpClient.GetAsync(url);
				if (!thumbnailResponse.IsSuccessStatusCode)
					continue;

#if HAS_UNO_SKIA_WPF
				var applicationFolder = Path.Combine(Windows.Storage.ApplicationData.Current.TemporaryFolder.Path, "UnoDrive");
				var imagesFolder = Path.Combine(applicationFolder, "thumbnails");
#else
				var imagesFolder = Path.Combine(Windows.Storage.ApplicationData.Current.LocalFolder.Path, "thumbnails");
#endif

				var name = $"{currentItem.Id}.jpeg";
				var localFilePath = Path.Combine(imagesFolder, name);

				try
				{
					if (!System.IO.Directory.Exists(imagesFolder))
						System.IO.Directory.CreateDirectory(imagesFolder);

					if (System.IO.File.Exists(localFilePath))
						System.IO.File.Delete(localFilePath);


					var bytes = await thumbnailResponse.Content.ReadAsByteArrayAsync();

#if HAS_UNO_SKIA_WPF
					System.IO.File.WriteAllBytes(localFilePath, bytes);
#else
					await System.IO.File.WriteAllBytesAsync(localFilePath, bytes);
#endif



#if __UNO_DRIVE_WINDOWS__ || __ANDROID__
					var image = new BitmapImage(new Uri(localFilePath));
#else
					var image = new BitmapImage();
					image.SetSource(new MemoryStream(bytes));
#endif

					childrenTable[currentItem.Id].ThumbnailSource = image;
				}
				catch (Exception ex)
				{
					logger.LogError(ex, ex.Message);
				}
			}

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
	}
}
