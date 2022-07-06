using System;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media.Animation;
using Microsoft.UI.Xaml.Navigation;
using UnoDrive.Mvvm;
using UnoDrive.Services;
using UnoDrive.ViewModels;

namespace UnoDrive.Views
{
	public sealed partial class Dashboard : Page
	{
		INavigationService navigation;
		public Dashboard()
		{
			this.InitializeComponent();
			contentFrame.Navigate(typeof(MyFilesPage), null, new SuppressNavigationTransitionInfo());
		}

		public DashboardViewModel ViewModel => (DashboardViewModel)DataContext;

		protected override async void OnNavigatedTo(NavigationEventArgs e)
		{
			base.OnNavigatedTo(e);

			if (e.Parameter is INavigationService navigation)
			{
				this.navigation = navigation;
			}

			if (ViewModel is IInitialize initializeViewModel)
			{
				await initializeViewModel.InitializeAsync();
			}
		}

		async void MenuItemSelected(NavigationView sender, NavigationViewItemInvokedEventArgs args)
		{
			if (signOut == args.InvokedItemContainer)
			{
				await navigation.SignOutAsync();
				return;
			}

			Type pageType = default;
			if (myFiles == args.InvokedItemContainer)
				pageType = typeof(MyFilesPage);
			else if (recentFiles == args.InvokedItemContainer)
				pageType = typeof(RecentFilesPage);
			else if (sharedFiles == args.InvokedItemContainer)
				pageType = typeof(SharedFilesPage);
			else if (recycleBin == args.InvokedItemContainer)
				pageType = typeof(RecycleBinPage);

			contentFrame.Navigate(pageType, null, new CommonNavigationTransitionInfo());
		}
	}
}
