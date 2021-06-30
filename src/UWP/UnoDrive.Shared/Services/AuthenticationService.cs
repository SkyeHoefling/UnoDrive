using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using UnoDrive.Models;

using MsalAuthenticationResult = Microsoft.Identity.Client.AuthenticationResult;
using MsalException = Microsoft.Identity.Client.MsalException;
using MsalIPublicClientApplication = Microsoft.Identity.Client.IPublicClientApplication;
using MsalUiRequiredException = Microsoft.Identity.Client.MsalUiRequiredException;

namespace UnoDrive.Services
{
    public interface IAuthenticationService
    {
        Task<IAuthenticationResult> AcquireSilentTokenAsync();
        Task<IAuthenticationResult> AcquireInteractiveTokenAsync();
        Task SignOutAsync();
    }

    public class AuthenticationService
    {
        
    }
}
