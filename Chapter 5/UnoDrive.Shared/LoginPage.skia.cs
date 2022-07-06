#if HAS_UNO_SKIA
namespace UnoDrive
{
	public partial class LoginPage
	{
		partial void SetHeaderText()
		{
			header.Text = "Hello from Skia";
		}
	}
}
#endif