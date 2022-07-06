using System.Threading.Tasks;
using System.Windows.Input;
using Microsoft.Extensions.Logging;
using Microsoft.Toolkit.Mvvm.Input;
using UnoDrive.Models;
using UnoDrive.Services;

namespace UnoDrive.ViewModels
{
	public class MyFilesViewModel : FilesViewModel
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
	}
}
