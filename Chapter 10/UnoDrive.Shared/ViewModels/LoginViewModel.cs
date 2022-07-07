using System.Windows.Input;
using Microsoft.Extensions.Logging;
using UnoDrive.Services;

#if __MACOS__ && !NET6_0_OR_GREATER
using Microsoft.Toolkit.Mvvm.Input;
#else
using CommunityToolkit.Mvvm.Input;
#endif

namespace UnoDrive.ViewModels
{
	public class LoginViewModel
    {
		INavigationService navigation;
		ILogger logger;

		public LoginViewModel(INavigationService navigation, ILogger<LoginViewModel> logger)
		{
			this.navigation = navigation;
			this.logger = logger;
			this.logger.LogInformation("Hello logging");

			Login = new RelayCommand(OnLogin);
		}

		public string Title => "Welcome to UnoDrive!";
		public string Header => "Uno Platform ♥ OneDrive = UnoDrive";
		public string ButtonText => "Login to UnoDrive";

		public ICommand Login { get; }

		void OnLogin() =>
			navigation.NavigateToDashboard();
	}
}
