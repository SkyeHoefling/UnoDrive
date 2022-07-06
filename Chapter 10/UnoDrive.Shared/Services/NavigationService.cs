using Microsoft.UI.Xaml.Controls;
using UnoDrive.Views;

namespace UnoDrive.Services
{
	public class NavigationService : INavigationService
	{
		public void NavigateToDashboard() =>
			GetRootFrame()?.Navigate(typeof(Dashboard), this);

		public void SignOut() =>
			GetRootFrame()?.GoBack();

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
