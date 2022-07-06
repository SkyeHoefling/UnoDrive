using System;
using System.Linq;
using System.Collections.ObjectModel;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using Microsoft.Graph;
using UnoDrive.Models;

namespace UnoDrive.ViewModels
{
    public class MyFilesViewModel : IAuthenticationProvider
    {
        public MyFilesViewModel()
        {
            LoadData();
            FilesAndFolders = new ObservableCollection<OneDriveItem>(new[]
            {
                new OneDriveItem
                {
                    Name = "Test",
                    FileSize = "100MB",
                    Modified = DateTime.Now,
                    ModifiedBy = "Andrew",
                    Sharing = ""
                },
                new OneDriveItem
                {
                    Name = "Test 1",
                    FileSize = "100MB",
                    Modified = DateTime.Now,
                    ModifiedBy = "Andrew",
                    Sharing = ""
                },
                new OneDriveItem
                {
                    Name = "Test 2",
                    FileSize = "100MB",
                    Modified = DateTime.Now,
                    ModifiedBy = "Andrew",
                    Sharing = ""
                },
                new OneDriveItem
                {
                    Name = "Test 3",
                    FileSize = "100MB",
                    Modified = DateTime.Now,
                    ModifiedBy = "Andrew",
                    Sharing = ""
                }
            });
        }

        public ObservableCollection<OneDriveItem> FilesAndFolders { get; set; }

        async void LoadData()
        {
#if __WASM__
            var httpClient = new HttpClient(new Uno.UI.Wasm.WasmHttpHandler());
#else
            var httpClient = new HttpClient();
#endif

            var graphClient = new GraphServiceClient(httpClient);
            graphClient.AuthenticationProvider = this;

            var rootChildren = await graphClient.Me.Drive.Root.Children
                .Request()
                .GetAsync();

            FilesAndFolders.Clear();
            foreach (var driveItem in rootChildren)
            {
                FilesAndFolders.Add(new OneDriveItem
                {
                    Name = driveItem.Name,
                    FileSize = $"{driveItem.Size}",
                    Modified = driveItem.LastModifiedDateTime.HasValue ?
                        driveItem.LastModifiedDateTime.Value.LocalDateTime : DateTime.Now,
                    Type = driveItem.Folder != null ? OneDriveItemType.Folder : OneDriveItemType.File
                    //ModifiedBy = driveItem.LastModifiedByUser.DisplayName,
                    //Sharing = ""
                });
            }
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
