using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using UnoDrive.Data;

namespace UnoDrive.Services
{
	public interface IGraphFileService
	{
		Task<IEnumerable<OneDriveItem>> GetRootFilesAsync(CancellationToken cancellationToken = default);
		Task<IEnumerable<OneDriveItem>> GetFilesAsync(string id, CancellationToken cancellationToken = default);
	}
}
