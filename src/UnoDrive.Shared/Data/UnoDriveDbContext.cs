using System.IO;
using Microsoft.EntityFrameworkCore;
using UnoDrive.Models;

namespace UnoDrive.Data
{
	public class UnoDriveDbContext : DbContext
	{
		public DbSet<OneDriveItem> OneDriveItems { get; set; }
		
		protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
		{
			// When building in app, use Windows.Storage.ApplicationData.Current.LocalFolder.Path
			// instead of /local to get browser persistence.


			// TODO - Get correct path for each platform.
			var local = Windows.Storage.ApplicationData.Current.LocalFolder.Path;
			var dbPath = Path.Combine(local, "unodrive.db");
			optionsBuilder.UseSqlite($"data source={dbPath}");
		}
	}
}