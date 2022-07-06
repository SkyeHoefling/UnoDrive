using System;
using System.IO;
using LiteDB;
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
			builder.WithIosKeychainSecurityGroup("com.AndrewHoefling.UnoDrive");
#endif

			var app = builder.Build();

			// this causes app to crash at startup
			// it appears msal.net doesn't compile against net6-android/ios/macos etc.
			// maybe we document these problems
#if !__ANDROID__ && !__IOS__
			TokenCacheStorage.EnableSerialization(app.UserTokenCache);
#endif

			services.AddSingleton(app);
			services.AddTransient<IAuthenticationService, AuthenticationService>();
		}

		private partial string GetRedirectUri();
	}
}
