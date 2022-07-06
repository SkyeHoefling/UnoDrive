using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

// The Blank Page item template is documented at http://go.microsoft.com/fwlink/?LinkId=402352&clcid=0x409

namespace UnoDrive
{
    public sealed partial class LoginPage : Page
    {
		public LoginPage()
		{
			this.InitializeComponent();
#if NETFX_CORE
			header.Text = "Hello from UWP";
#elif __ANDROID__
			header.Text = "Hello from Android";
#elif __IOS__
			header.Text = "Hello from iOS";
#elif __MACOS__
			header.Text = "Hello from macOS";
#elif HAS_UNO_WASM
			header.Text = "Hello from WASM";
#elif HAS_UNO_SKIA
			header.Text = "Hello from Skia";
#endif
		}

		partial void SetHeaderText();
	}
}
