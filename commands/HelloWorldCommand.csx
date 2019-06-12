#r "nuget: Newtonsoft.Json, 12.0.2"

#load "../dsx/IScriptCommand.csx"
#load "../dsx/IExecutionContext.csx"
#load "../AppSettings.csx"

using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System.Threading.Tasks;

public class HelloWorldCommand: IScriptCommand
{
    private readonly IExecutionContext context;
    private readonly ILogger<HelloWorldCommand> logger;
    private readonly AppSettings appSettings;

    public HelloWorldCommand(
        AppSettings appSettings,
        ILogger<HelloWorldCommand> logger,
        IExecutionContext context
        )
    {
        this.context = context;
        this.logger = logger;
        this.appSettings = appSettings;
    }

    public Task ExecuteAsync(string[] args)
    {
        Console.WriteLine("Hellow World!\n");
        
        if(String.IsNullOrEmpty(context.ScriptEnvironment))
        {
            logger.LogDebug("The script environment (DSX_ENVIRONMENT) is not specified");
        }
        else
        {
            logger.LogDebug($"The script environment (DSX_ENVIRONMENT) is: {context.ScriptEnvironment}");
        }

        if(args.Length > 0)
        {
            logger.LogDebug($"Command arguments: {String.Join(", ", args)}");
        }

        var scriptPath = context.GetScriptFilePath();
        logger.LogDebug($"Current script path is:\n      {scriptPath}");

        var appSettingsJson = JsonConvert.SerializeObject(appSettings, Formatting.Indented);
        logger.LogDebug($"Your current configuration is displayed below:\n      {appSettingsJson}");
        
        return Task.CompletedTask;
    }
}