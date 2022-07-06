using System.Linq;
using UnoDrive.Models;
using System.Collections.Generic;
using Windows.UI.Xaml.Controls;
using Microsoft.Toolkit.Mvvm.ComponentModel;
using UnoDrive.Services;

namespace UnoDrive.ViewModels
{
	public class MyFilesViewModel : ObservableObject
	{
		readonly IGraphFileService graphFileService;
		public MyFilesViewModel(IGraphFileService graphFileService)
		{
			this.graphFileService = graphFileService;

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
			if (args.ClickedItem is OneDriveItem driveItem)
			{
				if (driveItem.Type == OneDriveItemType.Folder)
				{
					// TODO - open folder
				}
				else
				{
					// TODO - open file
				}
			}
		}

		async void LoadData() =>
			FilesAndFolders = (await graphFileService.GetRootFiles()).ToList();
	}
}
