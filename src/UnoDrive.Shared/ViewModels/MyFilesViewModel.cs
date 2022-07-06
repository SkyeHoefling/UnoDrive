using System.Linq;
using UnoDrive.Models;
using System.Collections.Generic;
using System.Windows.Input;
using Windows.UI.Xaml.Controls;
using Microsoft.Toolkit.Mvvm.ComponentModel;
using Microsoft.Toolkit.Mvvm.Input;
using UnoDrive.Services;

namespace UnoDrive.ViewModels
{
	public class MyFilesViewModel : ObservableObject
	{
		Location location = new Location();
		readonly IGraphFileService graphFileService;
		public MyFilesViewModel(IGraphFileService graphFileService)
		{
			this.graphFileService = graphFileService;

			Forward = new RelayCommand(OnForward);
			Back = new RelayCommand(OnBack);

			FilesAndFolders = new List<OneDriveItem>();
			LoadData();
		}

		public ICommand Forward { get; }
		public ICommand Back { get; }

		// We are not using an ObservableCollection
		// by design. It can create significant performance
		// problems and it wasn't loading correctly on Android.
		List<OneDriveItem> filesAndFolders;
		public List<OneDriveItem> FilesAndFolders
		{
			get => filesAndFolders;
			set
			{
				SetProperty(ref filesAndFolders, value);
				OnPropertyChanged(nameof(CurrentFolderPath));
			}
		}

		public string CurrentFolderPath => FilesAndFolders.FirstOrDefault()?.Path;

		public async void ItemClick(object sender, ItemClickEventArgs args)
		{
			if (args.ClickedItem is OneDriveItem driveItem)
			{
				if (driveItem.Type == OneDriveItemType.Folder)
				{
					FilesAndFolders = (await graphFileService.GetFiles(driveItem.Id)).ToList();

					var firstItem = FilesAndFolders.FirstOrDefault();
					if (firstItem != null)
					{
						location.Forward = new Location 
						{
							Id = firstItem.PathId,
							Back = location
						};

						location = location.Forward;
					}
				}
				else
				{
					// TODO - open file
				}
			}
		}

		async void OnForward()
		{
			if (!location.CanMoveForward)
				return;

			FilesAndFolders = (await graphFileService.GetFiles(location.Forward.Id)).ToList();
			location = location.Forward;
		}

		async void OnBack()
		{
			if (!location.CanMoveBack)
				return;

			FilesAndFolders = (await graphFileService.GetFiles(location.Back.Id)).ToList();
			location = location.Back;
		}

		async void LoadData()
		{
			FilesAndFolders = (await graphFileService.GetRootFiles()).ToList();

			var firstItem = FilesAndFolders.FirstOrDefault();
			if (firstItem != null)
				location.Id = firstItem.PathId;
		}
	}
}
