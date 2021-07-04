using System;
using UnoDrive.Services;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Navigation;
using Uno.Extensions;

namespace UnoDrive.Views
{
	/// <summary>
	/// An empty page that can be used on its own or navigated to within a Frame.
	/// </summary>
	public sealed partial class Dashboard : Page
	{
		public Dashboard()
		{
			this.InitializeComponent();
			//contentFrame.Content = new MyFilesPage();
			//contentFrame.NavigateToType(typeof(MyFilesPage), null);
		}

		async void MenuItemSelected(NavigationView sender, NavigationViewItemInvokedEventArgs args)
		{
			//    var navOptions = new FrameNavigationOptions();
			//    navOptions.TransitionInfoOverride = args.RecommendedNavigationTransitionInfo;

			//    Type pageType = default;
			//    if (myFiles == args.InvokedItemContainer)
			//        pageType = typeof(MyFilesPage);
			//    else if (recentFiles == args.InvokedItemContainer)
			//        pageType = typeof(RecentFilesPages);
			//    else if (sharedFiles == args.InvokedItemContainer)
			//        pageType = typeof(SharedFilesPage);
			//    else if (recycleBin == args.InvokedItemContainer)
			//        pageType = typeof(RecycleBinPage);

			//    contentFrame.NavigateToType(pageType, null, navOptions);

			if (signOut == args.InvokedItemContainer)
			{
				var nav = (INavigationService)((App)App.Current).Container.GetService(typeof(INavigationService));
				await nav.SignOutAsync();
			}
		}
	}
}
