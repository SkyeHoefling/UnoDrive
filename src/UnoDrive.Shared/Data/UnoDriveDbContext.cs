using System.IO;
using Microsoft.EntityFrameworkCore;
using UnoDrive.Models;

namespace UnoDrive.Data
{
	public class UnoDriveDbContext : DbContext
	{
		public UnoDriveDbContext()
		{
			Database.EnsureCreated();
		}
		
		public DbSet<OneDriveItem> OneDriveItems { get; set; }
		public DbSet<Setting> Settings { get; set; }
		
		protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
		{
			// TODO - Get correct path for each platform.
			var local = Windows.Storage.ApplicationData.Current.LocalFolder.Path;
			var dbPath = Path.Combine(local, "unodrive.db");
			optionsBuilder.UseSqlite($"data source={dbPath}");
		}
	}
}