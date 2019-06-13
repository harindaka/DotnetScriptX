//#! "netcoreapp2.2"
#r "nuget: Microsoft.Extensions.Configuration, 2.2.0"
#r "nuget: Microsoft.Extensions.DependencyInjection, 2.2.0"

#load "AppSettingsFactory.csx"
#load "../AppSettings.csx"
#load "CommandRegistry.csx"
#load "../Startup.csx"
#load "IScriptCommand.csx"
#load "IExecutionContext.csx"
#load "ExecutionContext.csx"

using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.IO;
using System.Threading.Tasks;

public class Program
{    
    public static async Task Main(string[] args)
    {
        var commandName = String.Empty;
        if(args.Length > 0)
        {
            commandName = args[0];
        }

        IExecutionContext context = new ExecutionContext(
            commandName,
            Environment.GetEnvironmentVariable("DSX_ENVIRONMENT") ?? String.Empty
            );

        var appConfig = AppSettingsFactory.CreateInstance(context);
        var startup = new Startup(context, appConfig);

        IServiceCollection services = new ServiceCollection();

        services.AddSingleton<IConfiguration>(appConfig);

        services.AddSingleton<IExecutionContext>(context);

        startup.ConfigureServices(services);

        var commandRegistry = new CommandRegistry(services);
        startup.ConfigureCommands(commandRegistry);
        
        var serviceProvider = services.BuildServiceProvider();
        using (var scope = serviceProvider.CreateScope())
        {
            try
            {                
                var commandType = commandRegistry.DefaultCommandType;
                var commandArgs = new string[]{};
                
                if(!String.IsNullOrEmpty(commandName))
                {
                    commandType = commandRegistry.GetCommandType(commandName);
                    commandArgs = args.Skip(1).ToArray();
                }

                if(commandType == null)
                {
                    if(commandRegistry.OnCommandNotFoundAsync == null)
                    {
                        Console.WriteLine("Invalid command usage");
                        Environment.Exit(1);
                    }
                    else
                    {
                        commandType = await commandRegistry.OnCommandNotFoundAsync(commandName).ConfigureAwait(false);
                    }
                }

                var command = (IScriptCommand)scope.ServiceProvider.GetRequiredService(commandType); 
                
                await command.ExecuteAsync(commandArgs).ConfigureAwait(false);            
            }
            finally
            {
                (serviceProvider as IDisposable)?.Dispose();
            }
        }
    }
}

await Program.Main(Args.ToArray()).ConfigureAwait(false);