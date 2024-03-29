#r "nuget: Microsoft.Extensions.Configuration.Binder, 2.2.0"
#r "nuget: Serilog, 2.8.0"
#r "nuget: Serilog.Sinks.Console, 3.1.1"
#r "nuget: Serilog.Sinks.File, 4.0.0"
#r "nuget: Serilog.Settings.Configuration, 3.1.0"

#load "dsx/IScriptCommandCollection.csx"
#load "commands/HelloWorldCommand.csx"
#load "commands/UsageCommand.csx"

using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Serilog;
using Serilog.Settings.Configuration;

public class Startup
{
    private readonly IExecutionContext context;
    private readonly IConfiguration configuration;
    
    public Startup(
        IExecutionContext context, 
        IConfiguration configuration
        )
    {
        this.context = context;
        this.configuration = configuration;
    }

    public void ConfigureCommands(IScriptCommandCollection commands)
    {
        commands.RegisterDefault<UsageCommand>();
        commands.Register<UsageCommand>("help");

        commands.Register<HelloWorldCommand>("hello-world"); 

        commands.OnCommandNotFoundAsync = (commandName) => 
        {
            //Resolves the command type to be executed when 
            //users specify command names that are not registered 
            return Task.FromResult(typeof(UsageCommand));
        };
    }

    public void ConfigureServices(IServiceCollection services)
    {
        // Deserializes the IConfigurationRoot to the AppSettings POCO
        // and registers it with DI
        var appSettings = configuration.Get<AppSettings>();
        services.AddSingleton<AppSettings>(appSettings); 

        // Sets up Serilog using the configuration specified in appsettings.json
        // If you'd rather use a different logger remove this
        var thisProcess = System.Diagnostics.Process.GetCurrentProcess();
        var logger = new LoggerConfiguration()
                            .ReadFrom.Configuration(configuration)
                            .Enrich.WithProperty("CommandName", context.CommandName)
                            .Enrich.WithProperty("ProcessId", thisProcess.Id)
                            .Enrich.WithProperty("Environment", context.ScriptEnvironment)
                            .CreateLogger();
        
        services.AddSingleton<ILogger>(logger);
    }
}