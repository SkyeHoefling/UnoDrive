#if __WASM__
namespace UnoDrive
{
	public partial class LoginPage
	{
		partial void SetHeaderText()
		{
			header.Text = "Hello from WASM";
		}
	}
}
#endif