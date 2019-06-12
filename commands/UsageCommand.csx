#load "../dsx/IScriptCommand.csx"
#load "../dsx/IExecutionContext.csx"

using System.Threading.Tasks;
using System.Runtime.InteropServices;

public class UsageCommand : IScriptCommand
{
    private readonly IExecutionContext context;

    public UsageCommand(IExecutionContext context)
    {
        this.context = context;
    }

    public Task ExecuteAsync(string[] args)
    {
        Console.WriteLine();

        if(context.IsOSPlatform(OSPlatform.Windows))
        {
            Console.WriteLine("Usage: run [command] [arg1] [arg2] [arg3]");
            Console.WriteLine();
            Console.WriteLine("Examples:");
            Console.WriteLine("run hello-world powered by dsx");
            Console.WriteLine("set \"DSX_ENVIRONMENT=qa\" && run hello-world powered by dsx");
        }
        else
        {
            Console.WriteLine("Usage: ./run [command] [arg1] [arg2] [arg3]");
            Console.WriteLine();
            Console.WriteLine("Examples:");
            Console.WriteLine("./run hello-world powered by dsx");
            Console.WriteLine("DSX_ENVIRONMENT=qa ./run hello-world powered by dsx");
        }

        Console.WriteLine();
        Console.WriteLine("Available Commands:");
        Console.WriteLine("hello-world\t: Writes hello world to the console.");
        Console.WriteLine("help\t\t: Displays usage.");

        return Task.CompletedTask;
    }
}