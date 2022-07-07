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
				.Create("00e0a500-b640-4b46-81aa-8e3125ce932c")
				.WithRedirectUri(GetRedirectUri())
				.WithUnoHelpers();


			// WORKAROUND - This can be removed after we get a resolution to https://github.com/unoplatform/uno/discussions/7707
#if __ANDROID__
			builder.WithParentActivityOrWindow(() => Uno.UI.ContextHelper.Current as Android.App.Activity);
#elif __IOS__
			builder.WithParentActivityOrWindow(() => Microsoft.UI.Xaml.Window.Current.Content.Window.RootViewController);
#endif

#if __IOS__ || __MACOS__
			builder.WithIosKeychainSecurityGroup("com.SkyeHoefling.UnoDrive");
#endif

			services.AddSingleton(builder.Build());
			services.AddTransient<IAuthenticationService, AuthenticationService>();
		}

		private partial string GetRedirectUri();
	}
}
