#if __WASM__
namespace UnoDrive.Authentication
{
	public partial class AuthenticationConfiguration
    {
		private partial string GetRedirectUri() =>
			"https://localhost:5001/authentication/login-callback.htm";
    }
}
#endif
