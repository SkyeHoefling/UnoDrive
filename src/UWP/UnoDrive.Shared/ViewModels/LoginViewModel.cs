using System;
using System.Threading.Tasks;
using System.Windows.Input;
using Microsoft.Extensions.Logging;
using Microsoft.Toolkit.Mvvm.ComponentModel;
using Microsoft.Toolkit.Mvvm.Input;
using UnoDrive.Authentication;
using UnoDrive.Services;

namespace UnoDrive.ViewModels
{
    public class LoginViewModel : ObservableObject
    {
        readonly IAuthenticationService authentication;
        readonly INavigationService navigation;
        readonly ILogger logger;

        public LoginViewModel(
            IAuthenticationService authentication,
            INavigationService navigation,
            ILogger<LoginViewModel> logger)
        {
            this.authentication = authentication;
            this.navigation = navigation;
            this.logger = logger;

            Login = new AsyncRelayCommand(OnLogin);
        }

        public ICommand Login { get; }

        bool isBusy;
        public bool IsBusy
        {
            get => isBusy;
            set => SetProperty(ref isBusy, value);
        }

        string message;

        public string Message
        {
            get => message;
            set => SetProperty(ref message, value);
        }

        async Task OnLogin()
        {
            IsBusy = true;

            logger.LogInformation("Login tapped/clicked");

            try
            {
                var token = await authentication.AcquireInteractiveTokenAsync();
                ProcessAuthToken(token);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, ex.Message);

                // TODO - display error message to user.
            }
            finally
            {
                IsBusy = false;
            }
        }

        void ProcessAuthToken(IAuthenticationResult token)
        {
            if (token == null || !token.IsSuccess)
            {
                logger.LogError("Unable to log in null or unsuccessful retrieval");
                return;
            }

            if (!string.IsNullOrWhiteSpace(token.Message))
                Message = token.Message;

            navigation.NavigateToDashboard();
        }
    }
}
