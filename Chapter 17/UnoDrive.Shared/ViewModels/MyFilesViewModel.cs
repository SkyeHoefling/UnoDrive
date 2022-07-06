using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;
using Microsoft.Extensions.Logging;
using Microsoft.Toolkit.Mvvm.ComponentModel;
using Microsoft.Toolkit.Mvvm.Input;
using Microsoft.UI.Xaml.Controls;
using UnoDrive.Data;
using UnoDrive.Models;
using UnoDrive.Mvvm;
using UnoDrive.Services;

namespace UnoDrive.ViewModels
{
	public class MyFilesViewModel : ObservableObject, IInitialize
	{
		Location location = new Location();
		IGraphFileService graphFileService;
		ILogger logger;

		public MyFilesViewModel(
			IGraphFileService graphFileService,
			ILogger<MyFilesViewModel> logger)
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

		public async void OnItemClick(object sender, ItemClickEventArgs args)
		{
			if (args.ClickedItem is not OneDriveItem oneDriveItem)
				return;

			if (oneDriveItem.Type == OneDriveItemType.Folder)
			{
				try
				{
					await LoadDataAsync(oneDriveItem.Id);

					location.Forward = new Location
					{
						Id = oneDriveItem.Id,
						Back = location
					};
					location = location.Forward;
				}
				catch (Exception ex)
				{
					logger.LogError(ex, ex.Message);
				}
			}
		}

		Task OnForwardAsync()
		{
			if (!location.CanMoveForward)
				return Task.CompletedTask;

			var forwardId = location.Forward.Id;
			location = location.Forward;
			return LoadDataAsync(forwardId);
		}

		Task OnBackAsync()
		{
			if (!location.CanMoveBack)
				return Task.CompletedTask;

			var backId = location.Back.Id;
			location = location.Back;
			return LoadDataAsync(backId);
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
