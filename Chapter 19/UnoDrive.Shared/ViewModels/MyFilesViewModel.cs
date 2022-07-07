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
			Forward = new AsyncRelayCommand(OnForwardAsync);
			Back = new AsyncRelayCommand(OnBackAsync);
		}

		public ICommand Forward { get; }
		public ICommand Back { get; }

		Task OnForwardAsync()
		{
			if (!Location.CanMoveForward)
				return Task.CompletedTask;

			var forwardId = Location.Forward.Id;
			Location = Location.Forward;
			return LoadDataAsync(forwardId);
		}

		Task OnBackAsync()
		{
			if (!Location.CanMoveBack)
				return Task.CompletedTask;

			var backId = Location.Back.Id;
			Location = Location.Back;
			return LoadDataAsync(backId);
		}

		protected override Task<IEnumerable<OneDriveItem>> GetGraphDataAsync(string pathId, Action<IEnumerable<OneDriveItem>, bool> callback, CancellationToken cancellationToken) =>
			GraphFileService.GetMyFilesAsync(pathId, callback, cancellationToken);

		public override void OnItemClick(object sender, ItemClickEventArgs args) => base.OnItemClick(sender, args);

		public Task InitializeAsync() =>
			LoadDataAsync();
	}
}
