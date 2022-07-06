using Microsoft.UI.Xaml.Controls;

namespace UnoDrive
{
	public sealed partial class LoginPage : Page
    {
        public LoginPage()
        {
            this.InitializeComponent();

			// If you want to use platform specific C# uncomment the code below
			//SetHeaderText();
		}

		partial void SetHeaderText();
    }
}
