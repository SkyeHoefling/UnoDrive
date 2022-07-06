using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;

namespace UnoDrive.Views
{
	public sealed partial class LoginPage : Page
	{
		public LoginPage() =>
			this.InitializeComponent();

		void OnLoginClick(object sender, RoutedEventArgs args)
		{
			var window = ((App)App.Current).Window;
			if (window.Content is Frame rootFrame)
			{
				rootFrame.Navigate(typeof(Dashboard), null);
			}
		}
	}
}
