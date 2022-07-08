using System;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Navigation;
using UnoDrive.Mvvm;
using UnoDrive.ViewModels;

namespace UnoDrive.Views
{
	public sealed partial class MyFilesPage : Page
	{
		public MyFilesPage()
		{
			this.InitializeComponent();
			//SizeChanged += OnSizeChanged;
		}

		public MyFilesViewModel ViewModel => (MyFilesViewModel)DataContext;

		protected override async void OnNavigatedTo(NavigationEventArgs e)
		{
			base.OnNavigatedTo(e);
			
			if (ViewModel is IInitialize initializeViewModel)
				await initializeViewModel.InitializeAsync();
		}

		//void OnSizeChanged(object sender, SizeChangedEventArgs e)
		//{
		//	var window = ((App)App.Current).Window;
		//	scrollViewer.Height = window.Bounds.Height - rootGrid.RowDefinitions[0].ActualHeight;
		//}
	}
}