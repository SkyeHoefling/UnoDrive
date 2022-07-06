using Microsoft.Extensions.DependencyInjection;
using Microsoft.Identity.Client;
using Uno.UI.MSAL;

namespace UnoDrive.Authentication
{
	public partial class AuthenticationConfiguration
    {
		public void ConfigureAuthentication(IServiceCollection services)
		{
			// NOTE - 'WithUnoHelpers()' ensures correct
			// native code is ran on the various target
			// platforms.

			var builder = PublicClientApplicationBuilder
				.Create("a7410051-6505-4852-9b08-45a54d07c0bc")
				.WithRedirectUri(GetRedirectUri())
				.WithUnoHelpers();

			// WORKAROUND - This can be removed after we get a resolution to https://github.com/unoplatform/uno/discussions/7707
#if __ANDROID__
			builder.WithParentActivityOrWindow(() => Uno.UI.ContextHelper.Current as Android.App.Activity);
#endif

			services.AddSingleton(builder.Build());
			services.AddTransient<IAuthenticationService, AuthenticationService>();
		}

		private partial string GetRedirectUri();
    }
}
