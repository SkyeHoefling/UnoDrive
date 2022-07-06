using System.Threading.Tasks;

namespace UnoDrive.Services
{
	public interface INavigationService
	{
		void NavigateToDashboard();
		Task SignOutAsync();
	}
}
