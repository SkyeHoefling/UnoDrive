using System.Net.Http;
using Microsoft.Identity.Client;

namespace UnoDrive.Authentication
{
    public class MsalHttpClientFactory : IMsalHttpClientFactory
    {
        public HttpClient GetHttpClient()
        {
#if __WASM__
            var httpHandler = new Uno.UI.Wasm.WasmHttpHandler();
#else
            var httpHandler = new HttpClientHandler();
#endif

            return new HttpClient(httpHandler);
        }
    }
}
