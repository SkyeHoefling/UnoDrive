using System.Collections.Generic;
using UnoDrive.Data;

namespace UnoDrive.Services
{
	public interface ICachedGraphFileService
	{
		void SaveRootId(string rootId);
		string GetRootId();
		IEnumerable<OneDriveItem> GetCachedFiles(string pathId);
		void SaveCachedFiles(IEnumerable<OneDriveItem> children, string pathId);
		void UpdateCachedFileById(string itemId, string localFilePath);
	}
}
