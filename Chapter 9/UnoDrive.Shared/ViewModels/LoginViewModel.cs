using Microsoft.Extensions.Logging;

namespace UnoDrive.ViewModels
{
	public class LoginViewModel
    {
		ILogger logger;
		public LoginViewModel(ILogger<LoginViewModel> logger)
		{
			this.logger = logger;
			this.logger.LogInformation("Hello logging");
		}

		public string Title => "Welcome to UnoDrive!";
		public string Header => "Uno Platform ♥ OneDrive = UnoDrive";
		public string ButtonText => "Login to UnoDrive";
	}
}
