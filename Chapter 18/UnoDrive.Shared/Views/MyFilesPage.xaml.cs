using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Navigation;
using UnoDrive.Mvvm;
using UnoDrive.ViewModels;

namespace UnoDrive.Views
{
	public sealed partial class MyFilesPage : Page
	{
		public MyFilesPage() =>
			this.InitializeComponent();

		public MyFilesViewModel ViewModel => (MyFilesViewModel)DataContext;

		protected override async void OnNavigatedTo(NavigationEventArgs e)
		{
			base.OnNavigatedTo(e);

			if (ViewModel is IInitialize initializeViewModel)
				await initializeViewModel.InitializeAsync();
		}
	}
}