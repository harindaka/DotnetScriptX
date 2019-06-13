#r "nuget: Serilog, 2.8.0"
#r "nuget: Newtonsoft.Json, 12.0.2"

#load "../dsx/IScriptCommand.csx"
#load "../dsx/IExecutionContext.csx"
#load "../AppSettings.csx"

using Serilog;
using Newtonsoft.Json;
using System.Threading.Tasks;

public class HelloWorldCommand: IScriptCommand
{
    private readonly IExecutionContext context;
    private readonly ILogger logger;
    private readonly AppSettings appSettings;

    public HelloWorldCommand(
        AppSettings appSettings,
        ILogger logger,
        IExecutionContext context
        )
    {
        this.context = context;
        this.logger = logger;
        this.appSettings = appSettings;
    }

    public Task ExecuteAsync(string[] args)
    {
        var scriptPath = context.GetScriptFilePath();
        logger.Information("{0} command started", context.CommandName);

        Console.WriteLine("Hello World!");
        
        if(String.IsNullOrEmpty(context.ScriptEnvironment))
        {
            logger.Debug("The script environment (DSX_ENVIRONMENT) is not specified");
        }
        else
        {
            logger.Debug($"The script environment (DSX_ENVIRONMENT) is: {context.ScriptEnvironment}");
        }

        if(args.Length > 0)
        {
            logger.Debug($"Command arguments: {String.Join(", ", args)}");
        }
        
        logger.Debug($"Current script path is:\n{scriptPath}");

        var appSettingsJson = JsonConvert.SerializeObject(appSettings, Formatting.Indented);
        logger.Debug($"Your current configuration is displayed below:\n{appSettingsJson}");
        
        logger.Information("{0} command stopped", context.CommandName);

        return Task.CompletedTask;
    }
}