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
			// Android - add system.data
			// WASM - Follow Guide here https://github.com/unoplatform/Uno.SQLitePCLRaw.Wasm/blob/master/src/samples/EFCoreSample/EFCoreSample.Wasm/EFCoreSample.Wasm.csproj
			//      Run WSL Command - bash -c `wslpath "C:\Users\AndrewHoefling\.nuget\packages\uno.wasm.bootstrap\2.1.0\build\scripts\dotnet-setup.sh"`
			//      This is not supported in .net 5 - https://github.com/unoplatform/Uno.SQLitePCLRaw.Wasm/issues/19
			var local = Windows.Storage.ApplicationData.Current.LocalFolder.Path;
			var dbPath = Path.Combine(local, "unodrive.db");
			optionsBuilder.UseSqlite($"data source={dbPath}");
		}
	}
}