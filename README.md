# DotnetScriptX

## Dotnet-Script on Steroids!

DotnetScriptX is a project template / starter kit for the [dotnet-script](https://github.com/filipw/dotnet-script) tool. It provides the following additional features:

:heavy_check_mark: Loosely opinionated project template for writing and maintaining multiple asynchronous scripts with a default option for single script mode if you feel like it

:heavy_check_mark: Full blown Dependency Injection for scripting

:heavy_check_mark: Bootstrapper batch / bash scripts to execute script commands with automatic detection and installation of the dotnet-script tool

:heavy_check_mark: JSON based Application Configuration for scripts with environment specific overrides (e.g.: appsettings.prod.json) 

:heavy_check_mark: Script Execution Context with Current Script Path, Environment (Dev, Prod), OS detection helpers, and a standard configuration driven ILogger<> conveniently pre-registered for DI

## Quickstart

1. Download the latest template from the [releases](https://github.com/harindaka/DotnetScriptX/releases) section.
2. Extract the zip file and open a command prompt / terminal inside the extracted folder. The template includes two sample commands in the `commands` folder named `HelloWorldCommand.csx` and `UsageCommand.csx` for convenience. You can refer these when adding more commands which contains your own logic.
3. Execute the following command to view usage:

    Windows: 
    ```
    run
    ```
    Linux/MacOS:
    ```
    ./run
    ```

4. Seriously that's it! The aforementioned command will execute the default script command wired up for execution in `Startup.csx`. It will also try to detect and install [dotnet-script](https://github.com/filipw/dotnet-script) if not installed already. Tip: You may refer the `commands/UsageCommand.csx` to see how it works. 

As you may have already guessed, the DotnetScriptX template allows you to write and maintain multiple scripts in the same project folder in the form of `commands` (`IScriptCommand` implementations) 

## Creating Your Own Command

To create a new command all you have to do is implement the `IScriptCommand` interface and register it inside the `ConfigureCommands` method of the `Startup.csx` file

1. Just make a copy of the already provided `HelloWorldCommand.csx` inside the `commands` folder and rename the file to `YourCommand.csx`. Note that `YourCommand` can be anything else you like here. I just used that for simplicity's sake.

2. Open `YourCommand.csx` and rename the class and constructor names to `YourCommand`

3. Get rid of the code inside the `ExecuteAsync` method and add your own logic.

4. Remove any unnecessary `using` statements and `#r nuget:` references. Note that rules for writing scripts with [dotnet-script](https://github.com/filipw/dotnet-script) apply here. DSX does not introduce any additional paradigms.

5. Now open `Startup.csx` and load `YourCommand.csx` file into it by adding `#load "commands/YourCommand.csx"` directive at the top of the file.

```
#load "dsx/IScriptCommandCollection.csx"
#load "commands/HelloWorldCommand.csx"
#load "commands/UsageCommand.csx"
#load "commands/YourCommand.csx"
```

6. Register your command in the `ConfigureCommands` method like so: `commands.Register<YourCommand>("your-command");`

```
public void ConfigureCommands(IScriptCommandCollection commands)
{
    commands.RegisterDefault<UsageCommand>();
    commands.Register<UsageCommand>("help");

    commands.Register<HelloWorldCommand>("hello-world"); 

    commands.Register<YourCommand>("your-command");   
}
```

7. Congrats! you just implemented your first command. To run it, execute the following command:

    Windows: 
    ```
    run your-command
    ```
    Linux/MacOS:
    ```
    ./run your-command
    ```

## Single Script Mode

Note that inside the `Startup.csx`'s `ConfigureCommands` method, you can specify a default command to get executed when `run` is executed without any arguments. This is done using the `commands.RegisterDefault` method like so:

```
public void ConfigureCommands(IScriptCommandCollection commands)
{
    commands.RegisterDefault<YourCommand>();  
}
```

As in the example above, if you register `YourCommand` as the default command, when `run` is executed without parameters, `YourCommand` will get invoked. This is a good option if all you need is a quick way to execute a single script with a simple `run` command.

## Passing Arguments

1. You can pass command-line arguments to your commands with the `run` command like so:

    Windows:
    ```
    set "DSX_ENVIRONMENT=ye" && run your-command arg1 arg2 arg3
    ```
    
    Linux/MacOS:
    ```
    DSX_ENVIRONMENT=ye ./run your-command arg1 arg2 arg3
    ```

2. These will be available to `YourCommand` via the `args` parameter (string array) of the `ExecuteAsync` method.

## Handling Invalid Commands

Users may mistakenly issue commands which are not registered from time to time. You can handle them by specifying an `IScriptCommand` implementation type to be executed in such cases via the `ConfigureCommands` method of `Startup.csx`

```
public void ConfigureCommands(IScriptCommandCollection commands)
{
    commands.RegisterDefault<UsageCommand>();
    commands.Register<UsageCommand>("help");

    commands.Register<YourCommand>("your-command"); 

    commands.OnCommandNotFoundAsync = (commandName) => 
    {
        //Resolves the command type to be executed when 
        //users specify command names that are not registered 
        return Task.FromResult(typeof(UsageCommand));
    };
}
```

The `OnCommandNotFoundAsync` delegate makes the actual command name the user entered available as a method parameter.

## Dependency Injection
As mentioned previously, DI is already setup and ready to use via [`Microsoft.Extensions.DependencyInjection`](https://www.nuget.org/packages/Microsoft.Extensions.DependencyInjection/). To use this, register all your dependencies as follows:

1. Open `Startup.csx` and locate the `ConfigureServices` method
2. Register your dependency similar to how you would typically do it in ASP.Net using `IServiceCollection` parameter like so:

```
public void ConfigureServices(IServiceCollection services)
{
    var appSettings = configuration.Get<AppSettings>();
    services.AddSingleton<AppSettings>(appSettings);
}
```

Registered services would be automatically injected into your `IScriptCommand` implementation constructor like so:

```
public YourCommand(
        AppSettings appSettings,
        ILogger<HelloWorldCommand> logger,
        IExecutionContext context
    )
{        
    this.appSettings = appSettings;
    this.logger = logger;
    this.context = context;
}
```

Note that certain services such as `ILogger` and `IExecutionContext` are pre-registered for convenience.

## Application Configuration

You can manage configuration settings using `appsettings.json` files in the usual way with support for environment specific overrides as well. To add a setting,

1. Edit the provided `appsettings.json` and place your settings as a json object.
2. Change the provided `AppSettings` class to match.
3. Inject the `AppSettings` type into your command as a constructor parameter. Voila! Type safe application settings for your script!

```
public class YourCommand
{
    private readonly AppSettings appSettings;

    public YourCommand(AppSettings appSettings)
    {        
        this.appSettings = appSettings;
    }
}
```

Alternatively you can access the underlying `IConfiguration` object directly by injecting it into `YourCommand` too

```
public class YourCommand
{
    private readonly IConfiguration config;

    public YourCommand(IConfiguration config)
    {        
        this.config = config;
    }
}
```

### Environment Specific Configuration

You can also maintain environment specific configuration override files using this feature. Let's say you have an environment called `YourEnvironment`. (We'll use `ye` for short)

1. Your typical default settings may or may not reside in the base `appsettings.json` as follows:

```
{
    "ConnectionStrings": {
        "ConnectionString1": "Default Connection String"
    }
}
```

2. Add a environment specific configuration file titled `appsettings.ye.json` and override the same like so:

```
{
    "ConnectionStrings": {
        "ConnectionString1": "Your Environment Connection String"
    }
}
```

3. Set the environment variable `DSX_ENVIRONMENT` to the value `ye` and run `YourCommand`


    Windows:
    ```
    set "DSX_ENVIRONMENT=ye" && run your-command arg1 arg2 arg3
    ```
    
    Linux/MacOS:
    ```
    DSX_ENVIRONMENT=ye ./run your-command arg1 arg2 arg3
    ```

3. Thats it!. Now when you inject `AppSettings` or `IConfiguration` into `YourCommand`, the `ye` environment specific value should be available inside the `ConnectionString1` property.

Note that environment specific configuration files for common environments such as `prod`, `qa`, etc. are included for convenience.

## Script Execution Context
Contextual information about the current script being executed can be obtained by injecting the `IExecutionContext` service into `YourCommand`

```
public class YourCommand
{
    private readonly IExecutionContext context;

    public YourCommand(IExecutionContext context)
    {        
        this.context = context;
    }
}
```

| IExecutionContext Member| Description       |
| --------------------------| ------------------|
| `ScriptEnvironment`         | Returns the script environment under which the script is currently running. (The value specified by the `DSX_ENVIRONMENT` environment variable)|
| `IsOSPlatform(OSPlatform platform)`| Allows checking whether the current OS is one of the System.Runtime.InteropServices.OSPlatform values |
| `GetScriptFilePath()`| Returns the current script command file path
| `GetScriptFilePath()`| Returns the current script command file path|

Enjoy!

## License
MIT




 






