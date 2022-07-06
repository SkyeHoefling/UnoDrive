﻿using System;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.Identity.Client;
using Newtonsoft.Json;
using Uno.UI.MSAL;
using UnoDrive.Services;
using Windows.Networking.Connectivity;

namespace UnoDrive.Authentication
{
	public class AuthenticationService : IAuthenticationService
    {
		string[] scopes;

		IPublicClientApplication publicClientApp;
		INetworkConnectivityService networkService;
		ILogger logger;

		public AuthenticationService(
			IPublicClientApplication publicClientApp,
			INetworkConnectivityService networkService,
			ILogger<AuthenticationService> logger)
		{
			this.publicClientApp = publicClientApp;
			this.networkService = networkService;
			this.logger = logger;

			scopes = new[]
			{
				"email",
				"Files.ReadWrite.All",
				"offline_access",
				"profile",
				"User.Read"
			};
		}

		public async Task<AuthenticationResult> AcquireTokenAsync()
		{
			AuthenticationResult authentication = await AcquireSilentTokenAsync();
			if (authentication == null || string.IsNullOrEmpty(authentication.AccessToken))
			{
				authentication = await AcquireInteractiveTokenAsync();
			}

			return authentication;
		}

		public async Task SignOutAsync()
		{
			var accounts = await publicClientApp.GetAccountsAsync();
			var firstAccount = accounts.FirstOrDefault();
			if (firstAccount == null)
			{
				logger.LogInformation("Unable to find any accounts to log out of.");
				return;
			}

			await publicClientApp.RemoveAsync(firstAccount);
			logger.LogInformation($"Removed account: {firstAccount.Username}, user succesfully logged out.");
		}

		async Task<AuthenticationResult> AcquireInteractiveTokenAsync()
		{
			// NOTE - Using 'WithUnoHelpers()' is important to get the authentication
			// to work correctly across the various target platforms.

			return await publicClientApp
					.AcquireTokenInteractive(scopes)
					.WithUnoHelpers()
					.ExecuteAsync();
		}

		// NOTE - Original code note
		//
		// This won't work on all platforms and we will need to test it.
		// We may need to store the refresh token in a sqlite database and manually
		// request a new token
		async Task<AuthenticationResult> AcquireSilentTokenAsync()
		{
			var accounts = await publicClientApp?.GetAccountsAsync();
			var firstAccount = accounts.FirstOrDefault();

			if (firstAccount == null)
			{
				logger.LogInformation("Unable to find Account in MSAL.NET cache");
				return null;
			}

			if (accounts.Any())
			{
				logger.LogInformation($"Number of Accounts: {accounts.Count()}");
			}

			AuthenticationResult result = null;

			try
			{
				logger.LogInformation("Attempting to perform silent sign in . . .");
				logger.LogInformation($"Authentication Scopes: {JsonConvert.SerializeObject(scopes)}");
				logger.LogInformation($"Account Name: {firstAccount.Username}");

				result = await publicClientApp
					.AcquireTokenSilent(scopes, firstAccount)
					.WithForceRefresh(false)
					.ExecuteAsync();

				if (result != null && string.IsNullOrEmpty(result.AccessToken))
				{
					logger.LogInformation("Successfully acquired Access Token from silent sign in");
				}
			}
			catch (MsalUiRequiredException ex)
			{
				logger.LogWarning(ex, ex.Message);
				logger.LogWarning("Unable to retrieve silent sign in Access Token");
			}
			catch (HttpRequestException httpException)
			{
				if (networkService.Connectivity == NetworkConnectivityLevel.InternetAccess)
				{
					logger.LogError(httpException, httpException.Message);
				}
			}
			catch (Exception exception)
			{
				logger.LogWarning(exception, exception.Message);
				logger.LogWarning("Unable to retrieve silent sign in details");
			}
	
			return result;
		}
    }
}