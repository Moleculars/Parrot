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
        var service = GetService(args);
        service.Run();
    }

    public static ServiceRunnerBase GetService(string[] args)
    {
        return new ServiceRunner<Startup>(args);
    }


}