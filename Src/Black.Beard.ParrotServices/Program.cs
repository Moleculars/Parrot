using Bb;
using Bb.ParrotServices;

/// <summary>
/// Starting point of the application.
/// </summary>
public class Program
{

    /// <summary>
    /// Main entry point of the application.
    /// </summary>
    /// <param name="args"></param>
    public static void Main(string[] args)
    {            
        var service = new ServiceRunner<Startup>(args);
        service.Run();
    }


}