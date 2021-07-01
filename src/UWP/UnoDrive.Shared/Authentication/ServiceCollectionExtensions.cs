using Microsoft.Extensions.DependencyInjection;
using Microsoft.Identity.Client;
using Uno.UI.MSAL;

namespace UnoDrive.Authentication
{
    // For public use Azure B2C - https://docs.microsoft.com/en-us/azure/active-directory/develop/quickstart-register-app
    public static class ServiceCollectionExtensions
    {
        public static void UseAuthentication(this IServiceCollection services)
        {
            // Secret Sauce is to use UnoHelpers()

            var builder = PublicClientApplicationBuilder
                .Create("a7410051-6505-4852-9b08-45a54d07c0bc")
                .WithRedirectUri(GetRedirectUri())
                //.WithHttpClientFactory(new MsalHttpClientFactory())
                .WithUnoHelpers();

            services.AddSingleton(builder.Build());
            services.AddTransient<IAuthenticationService, AuthenticationService>();

            string GetRedirectUri()
            {
#if __WASM__
                return "https://localhost:5001/authentication/login-callback.htm";
#else
                return "unodrive://auth";
#endif
            }
        }
    }
}
