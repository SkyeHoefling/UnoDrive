using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using UnoDrive.Data;

namespace UnoDrive.Services
{
	public interface IGraphFileService
	{
		Task<IEnumerable<OneDriveItem>> GetRootFilesAsync();
		Task<IEnumerable<OneDriveItem>> GetFilesAsync(string id);
	}
}
