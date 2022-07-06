#if __MACOS__
namespace UnoDrive
{
	public partial class LoginPage
	{
		partial void SetHeaderText()
		{
			header.Text = "Hello from macOS";
		}
	}
}
#endif