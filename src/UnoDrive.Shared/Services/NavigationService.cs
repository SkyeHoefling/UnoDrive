using System.Threading.Tasks;
using UnoDrive.Views;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using UnoDrive.Authentication;

namespace UnoDrive.Services
{
    public interface INavigationService
    {
        void NavigateToDashboard();
        Task SignOutAsync();
    }

    public class NavigationService : INavigationService
    {
        readonly IAuthenticationService authenticationService;

        public NavigationService(IAuthenticationService authenticationService)
        {
            this.authenticationService = authenticationService;
        }

        public void NavigateToDashboard()
        {
            if (Window.Current.Content is Frame rootFrame)
            {
                rootFrame.Navigate(typeof(Dashboard), null);
            }
        }

        public async Task SignOutAsync()
        {
            if (Window.Current.Content is Frame rootFrame)
            {
                rootFrame.Navigate(typeof(LoginPage), null);
                await authenticationService.SignOutAsync();
            }
        }
    }
}
