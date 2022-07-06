using Windows.Networking.Connectivity;

namespace UnoDrive.Services
{
	public interface INetworkConnectivityService
    {
		NetworkConnectivityLevel Connectivity { get; }
	}
}
