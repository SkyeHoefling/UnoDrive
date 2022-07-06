using System.Windows.Input;
using Microsoft.Extensions.Logging;
using Microsoft.Toolkit.Mvvm.Input;

namespace UnoDrive.ViewModels
{
    public class LoginViewModel
    {
        readonly ILogger logger;
        public LoginViewModel(ILogger<LoginViewModel> logger)
        {
            this.logger = logger;
            Login = new RelayCommand(OnLogin);
        }

        public ICommand Login { get; }

        void OnLogin()
        {
            logger.LogInformation("Login tapped/clicked");
            System.Console.WriteLine("Perform login");
        }
    }
}
