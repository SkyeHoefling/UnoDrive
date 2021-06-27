using System;
using System.Collections.ObjectModel;
using UnoDrive.Models;

namespace UnoDrive.ViewModels
{
    public class MyFilesViewModel
    {
        public MyFilesViewModel()
        {
            FilesAndFolders = new ObservableCollection<ExplorerItem>(new[]
            {
                new ExplorerItem
                {
                    Name = "Test",
                    FileSize = "100MB",
                    Modified = DateTime.Now,
                    ModifiedBy = "Andrew",
                    Sharing = ""
                }
            });
        }

        public ObservableCollection<ExplorerItem> FilesAndFolders { get; set; }
    }
}
