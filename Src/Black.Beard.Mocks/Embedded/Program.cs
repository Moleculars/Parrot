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
    static void Main(string[] args)
    {

        if (args.Contains("--debug"))
        {

            if (!System.Diagnostics.Debugger.IsAttached)
                System.Diagnostics.Debugger.Launch();
        
        }

        var service = GetService(args);
        service.Run();
    
    }


    public static ServiceRunnerBase GetService(string[] args)
    {
        return new ServiceRunner<Startup>(args);
    }


}