using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;
using Microsoft.Extensions.Logging;
using Microsoft.Toolkit.Mvvm.ComponentModel;
using Microsoft.Toolkit.Mvvm.Input;
using UnoDrive.Data;
using UnoDrive.Exceptions;
using UnoDrive.Models;
using UnoDrive.Mvvm;
using UnoDrive.Services;
using Windows.UI.Xaml.Controls;

namespace UnoDrive.ViewModels
{
	public class MyFilesViewModel : ObservableObject, IInitialize
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
			}
			catch (Exception ex)
			{
				logger.LogError(ex, ex.Message);
			}

			void UpdateFiles(IEnumerable<OneDriveItem> files)
			{
				if (files == null)
				{
					logger.LogInformation("No data retrieved from API, ensure you have a stable internet connection");
					return;
				}

				FilesAndFolders = files.ToList();
			}
		}

		async Task IInitialize.InitializeAsync()
		{
			await LoadDataAsync(null, true);

			var firstItem = FilesAndFolders.FirstOrDefault();
			if (firstItem != null)
				location.Id = firstItem.PathId;
		}
	}
}
