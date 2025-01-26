using JpCommon;
using JupiterNeoServiceClient.Data;
using JupiterNeoServiceClient.Mappings;
using JupiterNeoServiceClient.Repositories.Implementations;
using JupiterNeoServiceClient.Repositories.Interfaces;
using JupiterNeoServiceClient.Utils;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

namespace JupiterNeoServiceClient
{
    public static class Program
    {
        public static void Main(string[] args)
        {
            CreateHostBuilder(args).Build().Run();
        }

        public static IHostBuilder CreateHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args)
                .UseWindowsService() // Configure the app to run as a Windows Service
                .ConfigureServices((hostContext, services) =>
                {
                    // Register your dependencies
                    services.AddHttpClient(); // Register HttpClient for DI
                    services.AddSingleton<JpCommon.JpApi>(); // Register BaseHttp

                    var sqlitePath = Path.Combine(new JpFilesManager().DiskPath!, JpCommon.JpConstants.ClientFolderName, JpCommon.JpConstants.SQLiteNameNeo);

                    services.AddDbContext<NeoDbContext>(options => options.UseSqlite($"DataSource={sqlitePath}"));
                    services.AddTransient<DbInit>();

                    services.AddScoped<IBackupRepository, SQLiteBackupRepository>();
                    services.AddScoped<ImetadataRepository, SQLiteMetadataRepository>();
                    services.AddScoped<IFileRepository, SQLiteFileRepository>();
                    services.AddScoped<IBackupPathRepository, SQLiteBakcupPathsRepository>();

                    services.AddAutoMapper(typeof(AutoMapperProfile));
                    services.AddHostedService<Service1>(); // Register Service1 as a Hosted Service


                });
    }
}
 