using UnoDrive.Mvvm;
using UnoDrive.ViewModels;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Navigation;

namespace UnoDrive.Views
{
	/// <summary>
	/// An empty page that can be used on its own or navigated to within a Frame.
	/// </summary>
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
