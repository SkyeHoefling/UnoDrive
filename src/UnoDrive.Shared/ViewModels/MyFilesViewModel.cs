using System.Linq;
using UnoDrive.Models;
using System.Collections.Generic;
using System.Windows.Input;
using Windows.UI.Xaml.Controls;
using Microsoft.Toolkit.Mvvm.ComponentModel;
using Microsoft.Toolkit.Mvvm.Input;
using UnoDrive.Data;
using UnoDrive.Services;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using System;
using UnoDrive.Exceptions;

namespace UnoDrive.ViewModels
{
	public class MyFilesViewModel : ObservableObject
	{
		Location location = new Location();
		readonly IGraphFileService graphFileService;
		readonly ILogger logger;
		public MyFilesViewModel(IGraphFileService graphFileService, ILogger<MyFilesViewModel> logger)
		{
			this.graphFileService = graphFileService;
			this.logger = logger;

			Forward = new AsyncRelayCommand(OnForwardAsync);
			Back = new AsyncRelayCommand(OnBackAsync);

			FilesAndFolders = new List<OneDriveItem>();
			LoadDataAsync(null, true);
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
					await LoadDataAsync(driveItem.Id);
					location.Forward = new Location
					{
						Id = driveItem.Id,
						Back = location
					};

					location = location.Forward;

				}
				else
				{
					// TODO - open file
				}
			}
		}

		async Task OnForwardAsync()
		{
			if (!location.CanMoveForward)
				return;

			// This doesn't appear to be working
			await LoadDataAsync(location.Forward.Id);
			location = location.Forward;
		}

		async Task OnBackAsync()
		{
			if (!location.CanMoveBack)
				return;

			await LoadDataAsync(location.Back.Id);
			location = location.Back;
		}

		async Task LoadDataAsync(string pathId = null, bool isFirstLoad = false)
		{
			try
			{
				IEnumerable<OneDriveItem> data;
				if (string.IsNullOrEmpty(pathId))
					data = await graphFileService.GetRootFiles(UpdateFiles);
				else
					data = await graphFileService.GetFiles(pathId, UpdateFiles);

				UpdateFiles(data);
				
				if (isFirstLoad)
				{
					// Configure the root pathId on first load
					var firstItem = FilesAndFolders.FirstOrDefault();
					if (firstItem != null)
						location.Id = firstItem.PathId;
				}
			}
			catch (NoDataException ex)
			{
				// TODO - Display warning to user that the data isn't available in offline mode.
				logger.LogError(ex, ex.Message);
			}
			catch (Exception ex)
			{
				logger.LogError(ex, ex.Message);
			}

			void UpdateFiles(IEnumerable<OneDriveItem> files)
			{
				if (files == null)
					return;

				FilesAndFolders = files.ToList();
			}
		}
	}
}
