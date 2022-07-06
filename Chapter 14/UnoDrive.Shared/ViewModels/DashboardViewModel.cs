using System;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.Graph;
using Microsoft.Toolkit.Mvvm.ComponentModel;
using Newtonsoft.Json;

namespace UnoDrive.ViewModels
{
	public class DashboardViewModel : ObservableObject, IAuthenticationProvider
    {
		ILogger logger;
		public DashboardViewModel(ILogger<DashboardViewModel> logger)
		{
			this.logger = logger;
			OnAppear();
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

		public async void OnAppear()
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

#if __ANDROID__
				var response = await request.GetResponseAsync();
				var data = await response.Content.ReadAsStringAsync();
				var me = JsonConvert.DeserializeObject<User>(data);
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
	}
}
