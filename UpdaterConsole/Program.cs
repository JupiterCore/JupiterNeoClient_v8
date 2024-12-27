using JupiterNeoUpdateService;

internal class Program
{
    static void Main(string[] args)
    {
        JpServiceUpdater jp = new JpServiceUpdater();
        jp.startService();

        Console.ReadKey();
    }
}
}
