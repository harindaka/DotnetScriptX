# DotnetScriptX

## Dotnet-Script on Steroids!

DotnetScriptX is a project template / starter kit for the [dotnet-script](https://github.com/filipw/dotnet-script) tool. It provides the following additional features:

:heavy_check_mark: Loosely opinionated project template for writing and maintaining multiple asynchronous scripts with a default option for single script mode if you feel like it

:heavy_check_mark: Full blown Dependency Injection for scripting

:heavy_check_mark: Bootstrapper batch / bash scripts to execute script commands with automatic detection and installation of the dotnet-script tool

:heavy_check_mark: JSON based Application Configuration for scripts with environment specific overrides (e.g.: appsettings.prod.json) 

:heavy_check_mark: Script Execution Context with Current Command Name, Script Path, Environment (Dev, Prod), OS detection helpers, and a configuration driven [Serilog](https://github.com/serilog/serilog) ILogger conveniently pre-registered for DI

:heavy_check_mark: Docker Support! Run or package your scripts inside a linux container with just a couple of commands.

## Quickstart

1. Download the latest project template (Source code archive) from the [releases](https://github.com/harindaka/DotnetScriptX/releases) section.
2. Extract the downloaded archive and open a command prompt / terminal inside the extracted folder. The template includes two sample commands in the `commands` folder named `HelloWorldCommand.csx` and `UsageCommand.csx` for convenience. You can refer these when adding more commands which contains your own logic.
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

## Default Command

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
        ILogger logger,
        IExecutionContext context
    )
{        
    this.appSettings = appSettings;
    this.logger = logger;
    this.context = context;
}
```

Note that certain services such as `IExecutionContext` are pre-registered for convenience.

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

| IExecutionContext Member | Description |
| -------------------------- | ------------------ |
| `CommandName`| Returns the current script command name |
| `ScriptEnvironment`| Returns the script environment under which the script is currently running. (The value specified by the `DSX_ENVIRONMENT` environment variable) |
| `IsOSPlatform(OSPlatform platform)`| Allows checking whether the current OS is one of the System.Runtime.InteropServices.OSPlatform values |
| `GetScriptFilePath()`| Returns the current script command file path |
| `GetScriptFilePath()`| Returns the current script command file path |


## Docker Support

You need to have docker installed (doh). There are two ways you can run your commands using docker.

1. Using the bash prompt inside a container (DockerfileBash): `Quick Image Build -> Slow Initial Command Execution -> Fast Subsequent Command Executions`. This approach is great if you are in development mode and quickly need a bash prompt to test out your changes without building a docker image and running it each time.

2. Package everything into a self contained docker image (DockerfilePack): `Slow Image Build -> Super Fast Command Execution`. This approach is a good fit for production environment where you want to build a docker image containing all your scripts and dependencies and deploy it into docker swarm, etc.

###  Using the bash prompt inside a container

1. Run the following command. This will build a docker image with the dotnet-script tool installed.

    Windows: 
    ```
    docker-bash-build
    ```
    Linux/MacOS:
    ```
    ./docker-bash-build
    ```

2. Start a container based on the image you just built with the following command.

    Windows: 
    ```
    docker-bash-run
    ```
    Linux/MacOS:
    ```
    ./docker-bash-run
    ```

3. You will be placed inside the bash prompt of the container. The above command maps your current project folder to the containers `/app` directory. From there you can run dsx in the usual manner mentioned previously. For e.g.:

    `./run hello-world arg1 arg2 arg3`

### Package everything into a self contained docker image

1. Run the following command. This will build a docker image including a copy of all the files in your current project directory inside it. It will also compile your scripts, publish them within the container and install the dotnet-script tool to ensure fast command execution.

    Windows: 
    ```
    docker-pack-build
    ```
    Linux/MacOS:
    ```
    ./docker-pack-build
    ```

2. You can run a short lived container per command in this mode. Commands and optionally arguments are specified as follows:

    Windows: 
    ```
    docker-pack-run your-command arg1 arg2 arg3
    ```
    Linux/MacOS:
    ```
    ./docker-pack-run your-command arg1 arg2 arg3
    ```

Feel free to edit the included dockerfiles `DockerfileBash` and `DockerfilePack` along with the corresponding bat / bash scripts to achieve what you need. E.g.: You can mount host directories as docker volumes inside the container to have access to the host file system.

## License
MIT

Hope this helps. If you find this useful don't forget the to spread the word!




 






