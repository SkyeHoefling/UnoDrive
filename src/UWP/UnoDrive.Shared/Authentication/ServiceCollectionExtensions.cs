﻿using Microsoft.Extensions.DependencyInjection;
using Microsoft.Identity.Client;

namespace UnoDrive.Authentication
{
    // For public use Azure B2C - https://docs.microsoft.com/en-us/azure/active-directory/develop/quickstart-register-app
    public static class ServiceCollectionExtensions
    {
        public static void UseAuthentication(this IServiceCollection services)
        {
            var builder = PublicClientApplicationBuilder
                .Create("a7410051-6505-4852-9b08-45a54d07c0bc")
                .WithRedirectUri("unodrive://auth");

            // TODO - implement for other platforms
#if __ANDROID__
			builder.WithParentActivityOrWindow(() => Windows.UI.Xaml.ApplicationActivity.Current);
#endif

            services.AddSingleton(builder.Build());
            services.AddTransient<IAuthenticationService, AuthenticationService>();
        }
    }
}