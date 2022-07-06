using Windows.Networking.Connectivity;

namespace UnoDrive.Services
{
	public class NetworkConnectivityService : INetworkConnectivityService
    {
		public NetworkConnectivityLevel Connectivity
		{
			get
			{
				var profile = NetworkInformation.GetInternetConnectionProfile();
				if (profile == null)
				{
					return NetworkConnectivityLevel.None;
				}

				return profile.GetNetworkConnectivityLevel();
			}
		}
    }
}
