using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using JupiterNeoServiceClient;
using JupiterNeoServiceClient.Controllers;
using System.IO;

namespace JupiterNeoServiceClient
{
    public class Program
    {
        public static void Main(string[] args)
        {
            CreateHostBuilder(args).Build().Run();
        }

        public static IHostBuilder CreateHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args)
                .UseWindowsService() // Configura para ejecutarse como un servicio de Windows.
                .ConfigureServices((hostContext, services) =>
                {
                    // Registro de servicios en el contenedor DI.
                    services.AddHostedService<BackupService>();

                    // Controladores necesarios para la lógica del servicio.
                    services.AddSingleton<SchedulesController>();
                    services.AddSingleton<FileController>();
                    services.AddSingleton<MetaDataController>();
                    services.AddSingleton<BackupPathController>();
                    services.AddSingleton<BackupPathModel>(); // Registrar el modelo asociado
                    services.AddSingleton<BaseController>();
                    services.AddSingleton<MetadataModel>();
                    services.AddSingleton<JpApi>();
                    services.AddSingleton<FileModel>();
                    services.AddSingleton<FileProcessor>();
                    services.AddSingleton<FileController>();

                     


                    // Registrar el DatabaseManager
                    services.AddSingleton<DatabaseManager>();

                    // Aquí puedes agregar otros servicios si los necesitas.
                })
                .ConfigureLogging(logging =>
                {
                    logging.ClearProviders(); // Limpia los proveedores por defecto.
                    logging.AddConsole(); // Agrega soporte para logs en consola (para depuración).
                    logging.AddEventLog(); // Agrega soporte para logs en el Visor de Eventos.
                });
    }
}
