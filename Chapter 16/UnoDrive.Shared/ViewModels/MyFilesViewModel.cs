using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using CommunityToolkit.Mvvm.ComponentModel;
using UnoDrive.Data;
using UnoDrive.Models;
using UnoDrive.Mvvm;
using UnoDrive.Services;

namespace UnoDrive.ViewModels
{
	public class MyFilesViewModel : ObservableObject, IInitialize
	{
		IGraphFileService graphFileService;
		ILogger logger;

		public MyFilesViewModel(
			IGraphFileService graphFileService,
			ILogger<MyFilesViewModel> logger)
		{
			this.graphFileService = graphFileService;
			this.logger = logger;

			FilesAndFolders = new List<OneDriveItem>();
		}

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
				OnPropertyChanged(nameof(IsPageEmpty));
			}
		}

		public bool IsPageEmpty => !IsStatusBarLoading && !FilesAndFolders.Any();

		public string CurrentFolderPath => FilesAndFolders.FirstOrDefault()?.Path;

		string noDataMessage;
		public string NoDataMessage
		{
			get => noDataMessage;
			set => SetProperty(ref noDataMessage, value);
		}

		bool isStatusBarLoading;
		public bool IsStatusBarLoading
		{
			get => isStatusBarLoading;
			set
			{
				SetProperty(ref isStatusBarLoading, value);
				OnPropertyChanged(nameof(IsPageEmpty));
			}
		}

		async Task LoadDataAsync(string pathId = null)
		{
			try
			{
				IsStatusBarLoading = true;

				IEnumerable<OneDriveItem> data;
				if (string.IsNullOrEmpty(pathId))
					data = await graphFileService.GetRootFilesAsync();
				else
					data = await graphFileService.GetFilesAsync(pathId);

				UpdateFiles(data);
			}
			catch (Exception ex)
			{
				logger.LogError(ex, ex.Message);
			}
			finally
			{
				IsStatusBarLoading = false;
			}
		}

		void UpdateFiles(IEnumerable<OneDriveItem> files)
		{
			if (files == null)
			{
				// This doesn't appear to be getting triggered correctly
				NoDataMessage = "Unable to retrieve data from API, check network connection";
				logger.LogInformation("No data retrieved from API, ensure you have a stable internet connection");
				return;
			}
			else if (!files.Any())
			{
				NoDataMessage = "No files or folders";
			}

			// TODO - The screen flashes briefly when loading the data from the API
			FilesAndFolders = files.ToList();
		}

		public async Task InitializeAsync()
		{
			await LoadDataAsync();
		}
	}
}
