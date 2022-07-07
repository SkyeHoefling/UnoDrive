using System;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.Graph;
using System.Text.Json;
using CommunityToolkit.Mvvm.ComponentModel;
using UnoDrive.Mvvm;

namespace UnoDrive.ViewModels
{
	public class DashboardViewModel : ObservableObject, IAuthenticationProvider, IInitialize
    {
		ILogger logger;
		public DashboardViewModel(ILogger<DashboardViewModel> logger)
		{
			this.logger = logger;
		}

		string name;
		public string Name
		{
			get => name;
			set => SetProperty(ref name, value);
		}

		string email;
		public string Email
		{
			get => email;
			set => SetProperty(ref email, value);
		}

		public async Task LoadDataAsync()
		{
			try
			{
#if __WASM__
				var httpClient = new HttpClient(new Uno.UI.Wasm.WasmHttpHandler());
#else
				var httpClient = new HttpClient();
#endif

				var graphClient = new GraphServiceClient(httpClient);
				graphClient.AuthenticationProvider = this;

				var request = graphClient.Me
					.Request()
					.Select(user => new
					{
						Id = user.Id,
						DisplayName = user.DisplayName,
						UserPrincipalName = user.UserPrincipalName
					});

#if __ANDROID__ || __IOS__ || __MACOS__
				var response = await request.GetResponseAsync();
				var data = await response.Content.ReadAsStringAsync();
				var me = JsonSerializer.Deserialize<User>(data);
#else
				var me = await request.GetAsync();
#endif

				if (me != null)
				{
					Name = me.DisplayName;
					Email = me.UserPrincipalName;
				}
			}
			catch (Exception ex)
			{
				logger.LogError(ex, ex.Message);
			}
		}

		public Task AuthenticateRequestAsync(HttpRequestMessage request)
		{
			var token = ((App)App.Current).AuthenticationResult?.AccessToken;
			if (string.IsNullOrEmpty(token))
				throw new Exception("No Access Token");

			request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
			return Task.CompletedTask;
		}

		public async Task InitializeAsync()
		{
			await LoadDataAsync();
		}
	}
}
