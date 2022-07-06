using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Input;
using Microsoft.Extensions.Logging;
using Microsoft.Toolkit.Mvvm.ComponentModel;
using Microsoft.Toolkit.Mvvm.Input;
using Microsoft.Toolkit.Uwp.Helpers;
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

		bool isMainFrameLoading;
		public bool IsMainFrameLoading
		{
			get => isMainFrameLoading;
			set => SetProperty(ref isMainFrameLoading, value);
		}

		bool isStatusBarLoading;
		public bool IsStatusBarLoading
		{
			get => isStatusBarLoading;
			set => SetProperty(ref isStatusBarLoading, value);
		}

		public async void ItemClick(object sender, ItemClickEventArgs args)
		{
			// TODO - Enable C# 9 for all projects
			if (!(args.ClickedItem is OneDriveItem driveItem))
				return;

			if (driveItem.Type == OneDriveItemType.Folder)
			{
				try
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
				catch (Exception ex)
				{
					logger.LogError(ex, ex.Message);
				}
			}
			else
			{
				// TODO - open file
			}
		}

		void OnNetworkStatusChanged(object sender) =>
			OnPropertyChanged(nameof(IsNetworkConnected));

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

			currentLoadDataTask = new TaskCompletionSource<bool>(TaskCreationOptions.RunContinuationsAsynchronously);
			cancellationTokenSource = new CancellationTokenSource();
			cancellationToken = cancellationTokenSource.Token;

			try
			{
				IsStatusBarLoading = true;

				IEnumerable<OneDriveItem> data;
				if (string.IsNullOrEmpty(pathId))
					data = await graphFileService.GetRootFilesAsync(UpdateFiles, cancellationToken);
				else
					data = await graphFileService.GetFilesAsync(pathId, UpdateFiles, cancellationToken);

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
				cancellationTokenSource = default;
				cancellationToken = default;

				IsMainFrameLoading = false;
				IsStatusBarLoading = false;

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
					IsMainFrameLoading = true;
					NoDataMessage = "No files or folders";
				}

				// TODO - The screen flashes briefly when loading the data from the API
				FilesAndFolders = files.ToList();
				IsMainFrameLoading = !files.Any();

				if (isCached)
					callback?.Invoke();
			}
		}

		async Task IInitialize.InitializeAsync()
		{
			RegisterEvents();
			await LoadDataAsync();

			var firstItem = FilesAndFolders.FirstOrDefault();
			if (firstItem != null)
				location.Id = firstItem.PathId;
		}

		void RegisterEvents()
		{
			// When using the Windows Community Toolkit WeakEventListener you can't
			// directly reference any object in `this.` context. Otherwise the GC
			// won't collect the resource correctly as it will create a tight coupling.
			// To work around this, creating a new locally scopped property creates
			// a new layer of indirection so the GC can properly clean up unused memory.
			// https://github.com/windows-toolkit/WindowsCommunityToolkit/issues/3029
			INetworkConnectivityService network = this.networkConnectivity;
			var weakEventListener = new WeakEventListener<MyFilesViewModel, object, object>(this)
			{
				OnEventAction = (instance, source, args) => instance.OnNetworkStatusChanged(source),
				OnDetachAction = (listener) => network.NetworkStatusChanged -= listener.OnEvent
			};
			network.NetworkStatusChanged += weakEventListener.OnEvent;
		}
	}
}
