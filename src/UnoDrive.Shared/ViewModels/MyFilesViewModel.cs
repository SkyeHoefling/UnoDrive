using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Input;
using Microsoft.Extensions.Logging;
using Microsoft.Toolkit.Mvvm.ComponentModel;
using Microsoft.Toolkit.Mvvm.Input;
using UnoDrive.Data;
using UnoDrive.Models;
using UnoDrive.Mvvm;
using UnoDrive.Services;
using Windows.Networking.Connectivity;
using Windows.UI.Xaml.Controls;

namespace UnoDrive.ViewModels
{
	public class MyFilesViewModel : ObservableObject, IInitialize
	{
		Location location = new Location();
		readonly IGraphFileService graphFileService;
		readonly ILogger logger;
		readonly INetworkConnectivityService networkConnectivity;
		public MyFilesViewModel(IGraphFileService graphFileService, ILogger<MyFilesViewModel> logger, INetworkConnectivityService networkConnectivity)
		{
			this.graphFileService = graphFileService;
			this.logger = logger;
			this.networkConnectivity = networkConnectivity;

			Forward = new AsyncRelayCommand(OnForwardAsync);
			Back = new AsyncRelayCommand(OnBackAsync);

			FilesAndFolders = new List<OneDriveItem>();
			this.networkConnectivity.NetworkStatusChanged += OnNetworkStatusChanged;
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

		public bool IsPageEmpty => !FilesAndFolders.Any();

		public string CurrentFolderPath => FilesAndFolders.FirstOrDefault()?.Path;

		public bool IsNetworkConnected =>
			networkConnectivity.Connectivity == NetworkConnectivityLevel.InternetAccess;

		string noDataMessage;
		public string NoDataMessage
		{
			get => noDataMessage;
			set => SetProperty(ref noDataMessage, value);
		}

		bool isBusy;
		public bool IsBusy
		{
			get => isBusy;
			set => SetProperty(ref isBusy, value);
		}

		bool isLoading;
		public bool IsLoading
		{
			get => isLoading;
			set => SetProperty(ref isLoading, value);
		}

		public async void ItemClick(object sender, ItemClickEventArgs args)
		{
			if (args.ClickedItem is OneDriveItem driveItem)
			{
				if (driveItem.Type == OneDriveItemType.Folder)
				{
					await LoadDataAsync(driveItem.Id, () =>
					{
						location.Forward = new Location
						{
							Id = driveItem.Id,
							Back = location
						};

						location = location.Forward;
					});

				}
				else
				{
					// TODO - open file
				}
			}
		}

		void OnNetworkStatusChanged(object sender) =>
			OnPropertyChanged(nameof(IsNetworkConnected));

		async Task OnForwardAsync()
		{
			if (!location.CanMoveForward)
				return;

			var forwardId = location.Forward.Id;
			location = location.Forward;
			await LoadDataAsync(forwardId);
		}

		async Task OnBackAsync()
		{
			if (!location.CanMoveBack)
				return;

			var backId = location.Back.Id;
			location = location.Back;
			await LoadDataAsync(backId);
		}

		CancellationTokenSource cancellationTokenSource;
		CancellationToken cancellationToken;
		TaskCompletionSource<bool> currentLoadDataTask;
		async Task LoadDataAsync(string pathId = null, Action callback = null)
		{
			if (cancellationTokenSource != null && !cancellationTokenSource.IsCancellationRequested)
			{
				cancellationTokenSource.Cancel();

				// This should prevent a race condition
				await currentLoadDataTask.Task;
			}

			currentLoadDataTask = new TaskCompletionSource<bool>();
			cancellationTokenSource = new CancellationTokenSource();
			cancellationToken = cancellationTokenSource.Token;

			try
			{
				IsLoading = true;

				IEnumerable<OneDriveItem> data;
				if (string.IsNullOrEmpty(pathId))
					data = await graphFileService.GetRootFiles(UpdateFiles, cancellationToken);
				else
					data = await graphFileService.GetFiles(pathId, UpdateFiles, cancellationToken);

				UpdateFiles(data);
			}
			catch (OperationCanceledException ex)
			{
				logger.LogInformation("API cancelled, user selected new file or folder");
				logger.LogInformation(ex, ex.Message);
			}
			catch (Exception ex)
			{
				logger.LogError(ex, ex.Message);
			}
			finally
			{
				cancellationTokenSource.Dispose();
				cancellationTokenSource = default;
				cancellationToken = default;

				IsBusy = false;
				IsLoading = false;

				currentLoadDataTask.SetResult(true);
			}

			void UpdateFiles(IEnumerable<OneDriveItem> files, bool isCached = false)
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
					IsBusy = true;
					NoDataMessage = "No files or folders";
				}

				// TODO - The screen flashes briefly when loading the data from the API
				FilesAndFolders = files.ToList();

				if (isCached)
					callback?.Invoke();
			}
		}

		async Task IInitialize.InitializeAsync()
		{
			await LoadDataAsync();

			var firstItem = FilesAndFolders.FirstOrDefault();
			if (firstItem != null)
				location.Id = firstItem.PathId;
		}
	}
}
