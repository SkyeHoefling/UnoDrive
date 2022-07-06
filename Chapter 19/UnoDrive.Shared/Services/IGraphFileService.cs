using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using UnoDrive.Data;

namespace UnoDrive.Services
{
	public interface IGraphFileService
	{
		Task<IEnumerable<OneDriveItem>> GetRootFilesAsync(Action<IEnumerable<OneDriveItem>, bool> cachedCallback = null, CancellationToken cancellationToken = default);
		Task<IEnumerable<OneDriveItem>> GetMyFilesAsync(string id, Action<IEnumerable<OneDriveItem>, bool> cachedCallback = null, CancellationToken cancellationToken = default);
		Task<IEnumerable<OneDriveItem>> GetRecentFilesAsync(Action<IEnumerable<OneDriveItem>, bool> cachedCallback = null, CancellationToken cancellationToken = default);
		Task<IEnumerable<OneDriveItem>> GetSharedFilesAsync(Action<IEnumerable<OneDriveItem>, bool> cachedCallback = null, CancellationToken cancellationToken = default);
		Task<IEnumerable<OneDriveItem>> GetRecycleBinFilesAsync(Action<IEnumerable<OneDriveItem>, bool> cachedCallback = null, CancellationToken cancellationToken = default);
	}
}
