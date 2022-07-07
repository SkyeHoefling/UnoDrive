using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Input;
using Microsoft.Extensions.Logging;
using Microsoft.UI.Xaml.Controls;
using CommunityToolkit.Mvvm.ComponentModel;
using UnoDrive.Data;
using UnoDrive.Models;
using UnoDrive.Mvvm;
using UnoDrive.Services;

namespace UnoDrive.ViewModels
{
	public class MyFilesViewModel : FilesViewModel, IInitialize
	{
		public MyFilesViewModel(
			IGraphFileService graphFileService,
			ILogger<MyFilesViewModel> logger) : base(graphFileService, logger)
		{
			Forward = new AsyncRelayCommand(OnForwardAsync, () => location.CanMoveForward);
			Back = new AsyncRelayCommand(OnBackAsync, () => location.CanMoveBack);
		}

		public IRelayCommand Forward { get; }
		public IRelayCommand Back { get; }

		Task OnForwardAsync()
		{
			var forwardId = Location.Forward.Id;
			Location = Location.Forward;
			return LoadDataAsync(forwardId);
		}

		Task OnBackAsync()
		{
			var backId = Location.Back.Id;
			Location = Location.Back;
			return LoadDataAsync(backId);
		}

		protected override Task<IEnumerable<OneDriveItem>> GetGraphDataAsync(string pathId, Action<IEnumerable<OneDriveItem>, bool> callback, CancellationToken cancellationToken) =>
			GraphFileService.GetMyFilesAsync(pathId, callback, cancellationToken);

		public override void OnItemClick(object sender, ItemClickEventArgs args) => base.OnItemClick(sender, args);

		protected override async Task LoadDataAsync(string pathId = null, Action presentationCallback = null)
		{
			await base.LoadDataAsync(pathId, presentationCallback);
			Forward.NotifyCanExecuteChanged();
			Back.NotifyCanExecuteChanged();
		}

		public Task InitializeAsync() =>
			LoadDataAsync();
	}
}
