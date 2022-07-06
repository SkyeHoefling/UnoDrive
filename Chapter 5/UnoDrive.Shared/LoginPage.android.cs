#if __ANDROID__
namespace UnoDrive
{
	public partial class LoginPage
	{
		partial void SetHeaderText()
		{
			header.Text = "Hello from Android";
		}
	}
}
#endif