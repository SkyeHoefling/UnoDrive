using System;
using System.Net.Http;
using System.Threading.Tasks;
using System.Windows.Input;
using Microsoft.Extensions.Logging;
using Microsoft.Identity.Client;
using UnoDrive.Authentication;
using UnoDrive.Services;
using Windows.Networking.Connectivity;

#if __MACOS__ && !NET6_0_OR_GREATER
using Microsoft.Toolkit.Mvvm.ComponentModel;
using Microsoft.Toolkit.Mvvm.Input;
#else
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
#endif

namespace UnoDrive.ViewModels
{
	public class LoginViewModel : ObservableObject
    {
		IAuthenticationService authentication;
		INetworkConnectivityService networkService;
		INavigationService navigation;
		ILogger logger;

		public LoginViewModel(
			IAuthenticationService authenticator,
			INetworkConnectivityService networkService,
			INavigationService navigation, 
			ILogger<LoginViewModel> logger)
		{
			this.authentication = authenticator;
			this.networkService = networkService;
			this.navigation = navigation;
			this.logger = logger;
			this.logger.LogInformation("Hello logging");

			Login = new AsyncRelayCommand(OnLoginAsync);
		}

		public string Title => "Welcome to UnoDrive!";
		public string Header => "Uno Platform ♥ OneDrive = UnoDrive";
		public string ButtonText => "Login to UnoDrive";

		public ICommand Login { get; }

		string message;
		public string Message
		{
			get => message;
			set => SetProperty(ref message, value);
		}

		bool isBusy;
		public bool IsBusy
		{
			get => isBusy;
			set => SetProperty(ref isBusy, value);
		}

		async Task OnLoginAsync()
		{
			IsBusy = true;
			logger.LogInformation("Login tapped or clicked");

			try
			{
				AuthenticationResult authenticationResult = await authentication.AcquireTokenAsync();
				if (authenticationResult == null || string.IsNullOrEmpty(authenticationResult.AccessToken))
				{
					logger.LogError("Unable to retrieve Access Token from Azure Active Directory");

					if (networkService.Connectivity != NetworkConnectivityLevel.InternetAccess)
					{
						logger.LogInformation("NO INTERNET CONNECTION: Internet required to retrieve an Access Token for the first time");
						Message = "No Internet, try again after connecting.";
					}
				}
				else
				{
					logger.LogInformation("Authentication successful, navigating to dashboard!");

					((App)App.Current).AuthenticationResult = authenticationResult;
					navigation.NavigateToDashboard();
				}
			}
			catch (MsalException msalException)
			{
				logger.LogError(msalException, msalException.Message);
				Message = msalException.Message;
			}
			catch (Exception exception)
			{
				logger.LogError(exception, exception.Message);
				Message = "Unable to sign-in, try again or check logs";
			}
			finally
			{
				IsBusy = false;
			}
		}
	}
}
