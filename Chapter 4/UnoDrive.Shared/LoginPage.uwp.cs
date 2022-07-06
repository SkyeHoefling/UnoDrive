#if NETFX_CORE
namespace UnoDrive
{
	public partial class LoginPage
	{
		partial void SetHeaderText()
		{
			header.Text = "Hello from UWP";
		}
	}
}
#endif