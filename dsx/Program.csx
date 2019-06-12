//#! "netcoreapp2.2"
#r "nuget: Microsoft.Extensions.Configuration, 2.2.0"
#r "nuget: Microsoft.Extensions.DependencyInjection, 2.2.0"
#r "nuget: Microsoft.Extensions.Logging.Console, 2.2.0"

#load "AppSettingsFactory.csx"
#load "../AppSettings.csx"
#load "CommandRegistry.csx"
#load "../Startup.csx"
#load "IScriptCommand.csx"
#load "IExecutionContext.csx"
#load "ExecutionContext.csx"

using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System;
using System.IO;
using System.Threading.Tasks;

public class Program
{    
    public static async Task Main(string[] args)
    {
        IExecutionContext context = new ExecutionContext(Environment.GetEnvironmentVariable("DSX_ENVIRONMENT") ?? String.Empty);
        var appConfig = AppSettingsFactory.CreateInstance(context);
        var startup = new Startup(appConfig);

        IServiceCollection services = new ServiceCollection();

        services.AddSingleton<IConfiguration>(appConfig);

        services.AddLogging(builder => 
        {
            builder.AddConfiguration(appConfig.GetSection("Logging"));
            builder.AddConsole();
        });

        services.AddSingleton<IExecutionContext>(context);

        startup.ConfigureServices(services);

        var commandRegistry = new CommandRegistry(services);
        startup.ConfigureCommands(commandRegistry);
        
        var serviceProvider = services.BuildServiceProvider();
        using (var scope = serviceProvider.CreateScope())
        {
            var logger = scope.ServiceProvider.GetRequiredService<ILogger<Program>>();
            try
            {                
                var commandType = commandRegistry.DefaultCommandType;
                var commandArgs = new string[]{};
                var commandName = String.Empty;
                if(args.Length > 0)
                {
                    commandName = args[0];
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
            catch (Exception exception) when (OnError(logger, exception))
            { 
                throw;
            }
            finally
            {
                (serviceProvider as IDisposable)?.Dispose();
            }
        }
    }

    private static bool OnError(ILogger<Program> logger, Exception ex)
    {
        logger.LogError(ex, "Error occured during script execution");
        return true;
    }
}

await Program.Main(Args.ToArray()).ConfigureAwait(false);