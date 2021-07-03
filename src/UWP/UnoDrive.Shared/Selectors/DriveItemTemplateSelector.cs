using UnoDrive.Models;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

namespace UnoDrive.Selectors
{
    public class DriveItemTemplateSelector : DataTemplateSelector
    {
        public DataTemplate FolderTemplate { get; set; }
        public DataTemplate ItemTemplate { get; set; }

        protected override DataTemplate SelectTemplateCore(object item)
        {
            var oneDriveItem = (OneDriveItem)item;
            if (oneDriveItem == null)
                return FolderTemplate;

            return oneDriveItem.Type == OneDriveItemType.Folder ?
                FolderTemplate : ItemTemplate;
        }
    }
}
