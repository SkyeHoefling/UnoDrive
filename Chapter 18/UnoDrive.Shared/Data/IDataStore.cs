using System.Collections.Generic;
using UnoDrive.Data;

namespace UnoDrive.Data
{
	public interface IDataStore
	{
		void SaveUserInfo(UserInfo userInfo);
		UserInfo GetUserInfoById(string userId);
		void SaveRootId(string rootId);
		string GetRootId();
		IEnumerable<OneDriveItem> GetCachedFiles(string pathId);
		void SaveCachedFiles(IEnumerable<OneDriveItem> children, string pathId);
		void UpdateCachedFileById(string itemId, string localFilePath);
	}
}
