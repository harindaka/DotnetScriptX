#r "nuget: Microsoft.Extensions.Configuration.Json, 2.2.0"

#load "IExecutionContext.csx"

using Microsoft.Extensions.Configuration;

public static class AppSettingsFactory
{
    public static IConfigurationRoot CreateInstance(IExecutionContext context)
    {
        var scriptPath = context.GetScriptDirectoryPath();
        scriptPath = Directory.GetParent(scriptPath).FullName;
        
        var configuration = new ConfigurationBuilder()
            .AddJsonFile(Path.Combine(scriptPath, "appsettings.json"), optional: true, reloadOnChange: true);

        if(!String.IsNullOrEmpty(context.ScriptEnvironment))
        {
            configuration.AddJsonFile(Path.Combine(scriptPath, $"appsettings.{context.ScriptEnvironment}.json"), optional: false, reloadOnChange: true);            
        }

        return configuration.Build();
    }
}