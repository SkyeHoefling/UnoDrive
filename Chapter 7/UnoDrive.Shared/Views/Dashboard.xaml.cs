using System;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media.Animation;

namespace UnoDrive.Views
{
	public sealed partial class Dashboard : Page
	{
		public Dashboard()
		{
			this.InitializeComponent();
			contentFrame.Navigate(typeof(MyFilesPage), null, new SuppressNavigationTransitionInfo());
		}

		void MenuItemSelected(NavigationView sender, NavigationViewItemInvokedEventArgs args)
		{
			// Signout is not implemented
			if (signOut == args.InvokedItemContainer)
				return;

			Type pageType = default;
			if (myFiles == args.InvokedItemContainer)
				pageType = typeof(MyFilesPage);
			else if (recentFiles == args.InvokedItemContainer)
				pageType = typeof(RecentFilesPage);
			else if (sharedFiles == args.InvokedItemContainer)
				pageType = typeof(SharedFilesPage);

			contentFrame.Navigate(pageType, null, new CommonNavigationTransitionInfo());
		}
	}
}
