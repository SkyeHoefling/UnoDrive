using UnoDrive.Views;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

namespace UnoDrive.Services
{
    public interface INavigationService
    {
        void NavigateToDashboard();
    }

    public class NavigationService : INavigationService
    {
        public void NavigateToDashboard()
        {
            if (Window.Current.Content is Frame rootFrame)
            {
                rootFrame.Navigate(typeof(Dashboard), null);
            }
        }
    }
}
