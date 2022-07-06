using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.UI.Xaml.Controls;
using UnoDrive.Data;
using UnoDrive.Mvvm;
using UnoDrive.Services;

namespace UnoDrive.ViewModels
{
	public class SharedFilesViewModel : FilesViewModel, IInitialize
	{
		public SharedFilesViewModel(
			IGraphFileService graphFileService,
			ILogger<SharedFilesViewModel> logger) : base(graphFileService, logger)
		{
		}

		public override string CurrentFolderPath => "Shared Files";

		protected override Task<IEnumerable<OneDriveItem>> GetGraphDataAsync(string pathId, Action<IEnumerable<OneDriveItem>, bool> callback, CancellationToken cancellationToken) =>
			GraphFileService.GetSharedFilesAsync(callback, cancellationToken);

		public override void OnItemClick(object sender, ItemClickEventArgs args) => base.OnItemClick(sender, args);

		public Task InitializeAsync() =>
			LoadDataAsync("SHARED-WITH-ME");
	}
}
