using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.Toolkit.Mvvm.ComponentModel;
using Microsoft.UI.Xaml.Controls;
using UnoDrive.Data;
using UnoDrive.Models;
using UnoDrive.Mvvm;
using UnoDrive.Services;

namespace UnoDrive.ViewModels
{
	public abstract class FilesViewModel : ObservableObject, IInitialize
	{
		protected Location Location { get; set; } = new Location();
		protected IGraphFileService GraphFileService { get; set; }
		protected ILogger Logger { get; set; }

		public FilesViewModel(
			IGraphFileService graphFileService,
			ILogger<MyFilesViewModel> logger)
		{
			GraphFileService = graphFileService;
			Logger = logger;

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
				OnPropertyChanged(nameof(IsMainContentLoading));
			}
		}

		public bool IsMainContentLoading => IsStatusBarLoading && !FilesAndFolders.Any();

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
				OnPropertyChanged(nameof(IsMainContentLoading));
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

					Location.Forward = new Location
					{
						Id = oneDriveItem.Id,
						Back = Location
					};
					Location = Location.Forward;
				}
				catch (Exception ex)
				{
					Logger.LogError(ex, ex.Message);
				}
			}
		}

		protected CancellationTokenSource CancellationTokenSource { get; set; }
		protected CancellationToken CancellationToken { get; set; }
		protected TaskCompletionSource<bool> CurrentLoadDataTask { get; set; }
		protected virtual async Task LoadDataAsync(string pathId = null, Action presentationCallback = null)
		{
			if (CancellationTokenSource != null && !CancellationTokenSource.IsCancellationRequested)
			{
				CancellationTokenSource.Cancel();

				// prevents race condition
				await CurrentLoadDataTask.Task;
			}

			CurrentLoadDataTask = new TaskCompletionSource<bool>(TaskCreationOptions.RunContinuationsAsynchronously);
			CancellationTokenSource = new CancellationTokenSource();
			CancellationToken = CancellationTokenSource.Token;

			try
			{
				IsStatusBarLoading = true;

				IEnumerable<OneDriveItem> data;
				Action<IEnumerable<OneDriveItem>, bool> updateFilesCallback = (items, isCached) => UpdateFiles(items, null, isCached);

				if (string.IsNullOrEmpty(pathId))
					data = await GraphFileService.GetRootFilesAsync(updateFilesCallback, CancellationToken);
				else
					data = await GraphFileService.GetMyFilesAsync(pathId, updateFilesCallback, CancellationToken);

				UpdateFiles(data, presentationCallback);
			}
			catch (Exception ex)
			{
				Logger.LogError(ex, ex.Message);
			}
			finally
			{
				CancellationTokenSource = default;
				CancellationToken = default;

				IsStatusBarLoading = false;

				CurrentLoadDataTask.SetResult(true);
			}
		}

		protected virtual void UpdateFiles(IEnumerable<OneDriveItem> files, Action presentationCallback, bool isCached = false)
		{
			if (files == null)
			{
				// This doesn't appear to be getting triggered correctly
				NoDataMessage = "Unable to retrieve data from API, check network connection";
				Logger.LogInformation("No data retrieved from API, ensure you have a stable internet connection");
				return;
			}
			else if (!files.Any())
			{
				NoDataMessage = "No files or folders";
			}

			// TODO - The screen flashes briefly when loading the data from the API
			FilesAndFolders = files.ToList();

			if (isCached)
			{
				presentationCallback?.Invoke();
			}
		}

		public async Task InitializeAsync()
		{
			await LoadDataAsync();
		}
	}
}
