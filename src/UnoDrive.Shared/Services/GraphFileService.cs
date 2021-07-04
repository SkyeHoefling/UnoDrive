using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using Microsoft.Graph;
using UnoDrive.Models;

namespace UnoDrive.Services
{
	public interface IGraphFileService
	{
		Task<IEnumerable<OneDriveItem>> GetRootFiles();
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

		public async Task<IEnumerable<OneDriveItem>> GetRootFiles()
		{
			var rootChildren = await graphClient.Me.Drive.Root.Children
				.Request()
				.GetAsync();

			return rootChildren
				.Select(driveItem => new OneDriveItem
				{
					Id = driveItem.Id,
					Name = driveItem.Name,
					FileSize = $"{driveItem.Size}",
					Modified = driveItem.LastModifiedDateTime.HasValue ?
						driveItem.LastModifiedDateTime.Value.LocalDateTime : DateTime.Now,
					Type = driveItem.Folder != null ? OneDriveItemType.Folder : OneDriveItemType.File
					//ModifiedBy = driveItem.LastModifiedByUser.DisplayName,
					//Sharing = ""
				});
		}

		public Task AuthenticateRequestAsync(HttpRequestMessage request)
		{
			var token = ((App)App.Current).AuthenticationResult?.AccessToken;
			if (string.IsNullOrEmpty(token))
				throw new System.Exception("No Access Token");

			request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
			return Task.CompletedTask;
		}
	}
}