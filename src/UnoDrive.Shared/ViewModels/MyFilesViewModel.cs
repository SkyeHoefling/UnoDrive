using System;
using System.Linq;
using System.Collections.ObjectModel;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using Microsoft.Graph;
using UnoDrive.Models;
using System.Collections.Generic;
using Windows.UI.Xaml.Controls;
using Microsoft.Toolkit.Mvvm.ComponentModel;

namespace UnoDrive.ViewModels
{
    public class MyFilesViewModel : ObservableObject, IAuthenticationProvider
    {
        public MyFilesViewModel()
        {
            FilesAndFolders = new List<OneDriveItem>();
            LoadData();
        }

        // We are not using an ObservableCollection
		// by design. It can create significant performance
		// problems and it wasn't loading correctly on Android.
        List<OneDriveItem> filesAndFolders;
        public List<OneDriveItem> FilesAndFolders
        {
            get => filesAndFolders;
            set => SetProperty(ref filesAndFolders, value);
        }
        
        public void ItemClick(object sender, ItemClickEventArgs args)
        {
        }

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

            FilesAndFolders = rootChildren
                .Select(driveItem => new OneDriveItem
                {
                    Name = driveItem.Name,
                    FileSize = $"{driveItem.Size}",
                    Modified = driveItem.LastModifiedDateTime.HasValue ?
                        driveItem.LastModifiedDateTime.Value.LocalDateTime : DateTime.Now,
                    Type = driveItem.Folder != null ? OneDriveItemType.Folder : OneDriveItemType.File
                    //ModifiedBy = driveItem.LastModifiedByUser.DisplayName,
                    //Sharing = ""
                })
                .ToList();
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
