
using MsalAuthenticationResult = Microsoft.Identity.Client.AuthenticationResult;

namespace UnoDrive.Authentication
{
	public static class AuthenticationResultExtensions
	{
		public static AuthenticationResult ToAuthenticationResult(this MsalAuthenticationResult authResult, string message = "", string objectId = "") =>
			new AuthenticationResult
			{
				AccessToken = authResult?.AccessToken,
				Message = message,
				ObjectId = objectId
			};
	}
}
