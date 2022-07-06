using System.Threading.Tasks;
using Microsoft.UI.Xaml.Controls;
using UnoDrive.Authentication;
using UnoDrive.Views;

namespace UnoDrive.Services
{
	public class NavigationService : INavigationService
	{
		IAuthenticationService authentication;
		public NavigationService(IAuthenticationService authentication)
		{
			this.authentication = authentication;
		}

		public void NavigateToDashboard() =>
			GetRootFrame().Navigate(typeof(Dashboard), this);

		public async Task SignOutAsync()
		{
			await authentication.SignOutAsync();
			GetRootFrame().Navigate(typeof(LoginPage), null);
		}

		Frame GetRootFrame()
		{
			var window = ((App)App.Current).Window;
			if (window.Content is Frame rootFrame)
			{
				return rootFrame;
			}

			return null;
		}
	}
}
