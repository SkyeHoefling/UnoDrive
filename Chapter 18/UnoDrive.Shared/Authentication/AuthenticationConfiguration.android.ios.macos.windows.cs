#if __ANDROID__ || __IOS__ || __MACOS__ || __UNO_DRIVE_WINDOWS__
namespace UnoDrive.Authentication
{
	public partial class AuthenticationConfiguration
    {
		private partial string GetRedirectUri() =>
			"unodrive://auth";
    }
}
#endif
