using UnoDrive.Services;

namespace UnoDrive.ViewModels
{
	public class DashboardViewModel
    {
		INavigationService navigation;
		public DashboardViewModel(INavigationService navigation)
		{
			this.navigation = navigation;
		}
	}
}
