using System.ServiceProcess;

namespace JupiterNeoUpdateService
{
    internal static class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        static void Main()
        {
            ServiceBase[] ServicesToRun;
            ServicesToRun = new ServiceBase[]
            {
                new JpServiceUpdater()
            };
            ServiceBase.Run(ServicesToRun);
        }
    }
}
