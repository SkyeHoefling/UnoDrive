#if NET6_0_OR_GREATER && WINDOWS
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