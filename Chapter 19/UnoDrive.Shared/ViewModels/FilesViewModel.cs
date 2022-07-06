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
	public abstract class FilesViewModel : ObservableObject
	{
		protected Location Location { get; set; } = new Location();
		protected IGraphFileService GraphFileService { get; set; }
		protected ILogger Logger { get; set; }

		public FilesViewModel(
			IGraphFileService graphFileService,
			ILogger<FilesViewModel> logger)
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

		public virtual string CurrentFolderPath => FilesAndFolders.FirstOrDefault()?.Path;

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

		public async virtual void OnItemClick(object sender, ItemClickEventArgs args)
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

		protected abstract Task<IEnumerable<OneDriveItem>> GetGraphDataAsync(string pathId, Action<IEnumerable<OneDriveItem>, bool> callback, CancellationToken cancellationToken);

		CancellationTokenSource cancellationTokenSource;
		CancellationToken cancellationToken;
		TaskCompletionSource<bool> currentLoadDataTask;
		protected async Task LoadDataAsync(string pathId = null, Action presentationCallback = null)
		{
			if (cancellationTokenSource != null && !cancellationTokenSource.IsCancellationRequested)
			{
				cancellationTokenSource.Cancel();

				// prevents race condition
				await currentLoadDataTask.Task;
			}

			currentLoadDataTask = new TaskCompletionSource<bool>(TaskCreationOptions.RunContinuationsAsynchronously);
			cancellationTokenSource = new CancellationTokenSource();
			cancellationToken = cancellationTokenSource.Token;

			try
			{
				IsStatusBarLoading = true;

				IEnumerable<OneDriveItem> data;
				Action<IEnumerable<OneDriveItem>, bool> updateFilesCallback = (items, isCached) => UpdateFiles(items, null, isCached);

				if (string.IsNullOrEmpty(pathId))
					data = await GraphFileService.GetRootFilesAsync(updateFilesCallback, cancellationToken);
				else
					data = await GetGraphDataAsync(pathId, updateFilesCallback, cancellationToken);

				UpdateFiles(data, presentationCallback);
			}
			catch (Exception ex)
			{
				Logger.LogError(ex, ex.Message);
			}
			finally
			{
				cancellationTokenSource = default;
				cancellationToken = default;

				IsStatusBarLoading = false;

				currentLoadDataTask.SetResult(true);
			}
		}

		protected void UpdateFiles(IEnumerable<OneDriveItem> files, Action presentationCallback, bool isCached = false)
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

		
	}
}
