using Windows.UI.Xaml.Controls;
using UnoDrive.ViewModels;

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
	}
}
