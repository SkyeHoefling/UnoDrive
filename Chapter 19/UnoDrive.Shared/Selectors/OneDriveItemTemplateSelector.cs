using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using UnoDrive.Data;

namespace UnoDrive.Selectors
{
	public class OneDriveItemTemplateSelector : DataTemplateSelector
    {
		public DataTemplate FolderTemplate { get; set; }
		public DataTemplate ItemTemplate { get; set; }

		protected override DataTemplate SelectTemplateCore(object item)
		{
			if (item is not OneDriveItem oneDriveItem)
				return FolderTemplate;

			return oneDriveItem.Type == OneDriveItemType.Folder ?
				FolderTemplate : ItemTemplate;
		}
	}
}
