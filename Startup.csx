#r "nuget: Microsoft.Extensions.Configuration.Binder, 2.2.0"

#load "dsx/IScriptCommandCollection.csx"
#load "commands/HelloWorldCommand.csx"
#load "commands/UsageCommand.csx"

using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

public class Startup
{
    private readonly IConfiguration configuration;
    
    public Startup(IConfiguration configuration)
    {
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
        var appSettings = configuration.Get<AppSettings>();
        services.AddSingleton<AppSettings>(appSettings);        
    }
}