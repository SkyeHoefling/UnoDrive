using System.Threading.Tasks;
using Microsoft.Identity.Client;

namespace UnoDrive.Authentication
{
	public interface IAuthenticationService
    {
		Task<AuthenticationResult> AcquireTokenAsync();
		Task SignOutAsync();
	}
}
