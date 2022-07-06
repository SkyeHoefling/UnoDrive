using Microsoft.UI.Xaml.Controls;

namespace UnoDrive
{
	public sealed partial class LoginPage : Page
    {
		public LoginPage()
		{
			this.InitializeComponent();
#if NET6_0_OR_GREATER && WINDOWS
			header.Text = "Hello from Windows";
#elif __ANDROID__
			header.Text = "Hello from Android";
#elif __IOS__
			header.Text = "Hello from iOS";
#elif __MACOS__
			header.Text = "Hello from macOS";
#elif __WASM__
			header.Text = "Hello from WASM";
#elif HAS_UNO_SKIA
			header.Text = "Hello from Skia";
#endif
		}

		partial void SetHeaderText();
	}
}
