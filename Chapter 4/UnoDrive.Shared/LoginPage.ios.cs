#if __IOS__
namespace UnoDrive
{
	public partial class LoginPage
	{
		partial void SetHeaderText()
		{
			header.Text = "Hello from iOS";
		}
	}
}
#endif