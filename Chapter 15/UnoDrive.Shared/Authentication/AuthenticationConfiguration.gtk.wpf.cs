#if HAS_UNO_SKIA
namespace UnoDrive.Authentication
{
	public partial class AuthenticationConfiguration
    {
		private partial string GetRedirectUri() =>
			"http://localhost:9471";
    }
}
#endif
