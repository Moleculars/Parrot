using Bb;
using Bb.ParrotServices;

internal class Program
{


    public static void Main(string[] args)
    {
        var service = new ServiceRunner<Startup>(args);
        service.Run();
    }

    
}