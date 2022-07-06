using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.Graph;
using Microsoft.Toolkit.Mvvm.ComponentModel;

namespace UnoDrive.ViewModels
{
    public class DashboardViewModel : ObservableObject, IAuthenticationProvider
    {
        readonly ILogger logger;
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

        async void OnAppear()
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

                // doesn't work on WASM (use http instead)
                var me = await graphClient.Me
                    .Request()
                    .Select(u => new
                    {
                        Id = u.Id,
                        DisplayName = u.DisplayName,
                        UserPrincipalName = u.UserPrincipalName
                    })
                    .GetAsync();

                if (me != null)
                {
                    Name = me.DisplayName;
                    Email = me.UserPrincipalName;
                }
            }
            catch (System.Exception ex)
            {
                logger.LogError(ex, ex.Message);
            }
        }

        public Task AuthenticateRequestAsync(HttpRequestMessage request)
        {
            var token = ((App)App.Current).AuthenticationResult?.AccessToken;
            if (string.IsNullOrEmpty(token))
                throw new System.Exception("No Access Token");

            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
            return Task.CompletedTask;
        }
    }
}
