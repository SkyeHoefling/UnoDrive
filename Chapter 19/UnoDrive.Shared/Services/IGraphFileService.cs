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
		Task<IEnumerable<OneDriveItem>> GetFilesAsync(string id, Action<IEnumerable<OneDriveItem>, bool> cachedCallback = null, CancellationToken cancellationToken = default);
	}
}
