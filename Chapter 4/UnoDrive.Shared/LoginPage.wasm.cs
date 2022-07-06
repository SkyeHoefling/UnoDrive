#if HAS_UNO_WASM
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