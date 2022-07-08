using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Input;
using Microsoft.Extensions.Logging;
using Microsoft.UI.Xaml.Controls;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
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

			Forward = new AsyncRelayCommand(OnForwardAsync, () => location.CanMoveForward);
			Back = new AsyncRelayCommand(OnBackAsync, () => location.CanMoveBack);

			FilesAndFolders = new List<OneDriveItem>();
		}

		public IRelayCommand Forward { get; }
		public IRelayCommand Back { get; }

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
					location.Forward = new Location
					{
						Id = oneDriveItem.Id,
						Back = location
					};
					location = location.Forward;

					await LoadDataAsync(oneDriveItem.Id);
				}
				catch (Exception ex)
				{
					logger.LogError(ex, ex.Message);
				}
			}
		}

		Task OnForwardAsync()
		{
			var forwardId = location.Forward.Id;
			location = location.Forward;
			return LoadDataAsync(forwardId);
		}

		Task OnBackAsync()
		{
			var backId = location.Back.Id;
			location = location.Back;
			return LoadDataAsync(backId);
		}

		CancellationTokenSource cancellationTokenSource;
		TaskCompletionSource<bool> currentLoadDataTask;
		async Task LoadDataAsync(string pathId = null, Action presentationCallback = null)
		{
			if (cancellationTokenSource != null && !cancellationTokenSource.IsCancellationRequested)
			{
				cancellationTokenSource.Cancel();

				// prevents race condition
				await currentLoadDataTask.Task;
			}

			currentLoadDataTask = new TaskCompletionSource<bool>(TaskCreationOptions.RunContinuationsAsynchronously);
			cancellationTokenSource = new CancellationTokenSource();
			var cancellationToken = cancellationTokenSource.Token;

			try
			{
				IsStatusBarLoading = true;

				IEnumerable<OneDriveItem> data;
				Action<IEnumerable<OneDriveItem>, bool> updateFilesCallback = (items, isCached) => UpdateFiles(items, null, isCached);

				if (string.IsNullOrEmpty(pathId))
					data = await graphFileService.GetRootFilesAsync(updateFilesCallback, cancellationToken);
				else
					data = await graphFileService.GetFilesAsync(pathId, updateFilesCallback, cancellationToken);

				UpdateFiles(data, presentationCallback);
			}
			catch (Exception ex)
			{
				logger.LogError(ex, ex.Message);
			}
			finally
			{
				cancellationTokenSource = default;
				cancellationToken = default;

				Forward.NotifyCanExecuteChanged();
				Back.NotifyCanExecuteChanged();

				IsStatusBarLoading = false;

				currentLoadDataTask.SetResult(true);
			}
		}

		void UpdateFiles(IEnumerable<OneDriveItem> files, Action presentationCallback, bool isCached = false)
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
