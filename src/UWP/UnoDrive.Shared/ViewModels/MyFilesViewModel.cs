using System;
using System.Collections.ObjectModel;
using UnoDrive.Models;

namespace UnoDrive.ViewModels
{
    public class MyFilesViewModel
    {
        public MyFilesViewModel()
        {
            FilesAndFolders = new ObservableCollection<OneDriveItem>(new[]
            {
                new OneDriveItem
                {
                    Name = "Test",
                    FileSize = "100MB",
                    Modified = DateTime.Now,
                    ModifiedBy = "Andrew",
                    Sharing = ""
                },
                new OneDriveItem
                {
                    Name = "Test 1",
                    FileSize = "100MB",
                    Modified = DateTime.Now,
                    ModifiedBy = "Andrew",
                    Sharing = ""
                },
                new OneDriveItem
                {
                    Name = "Test 2",
                    FileSize = "100MB",
                    Modified = DateTime.Now,
                    ModifiedBy = "Andrew",
                    Sharing = ""
                },
                new OneDriveItem
                {
                    Name = "Test 3",
                    FileSize = "100MB",
                    Modified = DateTime.Now,
                    ModifiedBy = "Andrew",
                    Sharing = ""
                }
            });
        }

        public ObservableCollection<OneDriveItem> FilesAndFolders { get; set; }
    }
}
