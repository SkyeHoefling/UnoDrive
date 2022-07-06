using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using UnoDrive.Data;
using UnoDrive.Mvvm;
using UnoDrive.Services;

namespace UnoDrive.ViewModels
{
	public class RecycleBinViewModel : FilesViewModel, IInitialize
	{
		public RecycleBinViewModel(
			IGraphFileService graphFileService,
			ILogger<MyFilesViewModel> logger) : base(graphFileService, logger)
		{
		}

		protected override Task<IEnumerable<OneDriveItem>> GetGraphDataAsync(string pathId, Action<IEnumerable<OneDriveItem>, bool> callback, CancellationToken cancellationToken) =>
			GraphFileService.GetRecycleBinFilesAsync(callback, cancellationToken);

		public Task InitializeAsync() =>
			LoadDataAsync("RECENT");
	}
}
