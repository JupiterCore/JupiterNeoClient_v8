using JupiterNeoServiceClient;

namespace ClientConsole
{
    internal class Program
    {
        static void Main(string[] args)
        {
            Service1 service = new Service1();
            service.OnServiceStart();
            Console.ReadKey();
        }
    }
}
