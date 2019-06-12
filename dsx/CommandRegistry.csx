#r "nuget: Microsoft.Extensions.DependencyInjection, 2.2.0"

#load "IScriptCommand.csx"
#load "IScriptCommandCollection.csx"

using Microsoft.Extensions.DependencyInjection;

public class CommandRegistry: IScriptCommandCollection
{
    private readonly IServiceCollection services;
    private readonly IDictionary<string, Type> commands;
    private Type defaultCommandType;
    
    public CommandRegistry(IServiceCollection services)
    {
        this.services = services;
        this.commands = new Dictionary<string, Type>();
    }
    
    public Type DefaultCommandType => defaultCommandType;

    public Func<string, Task<Type>> OnCommandNotFoundAsync { get; set; }
    
    public void Register<TCommand>(string commandName) where TCommand: IScriptCommand
    {        
        var commandType = typeof(TCommand);
        commands[commandName] = commandType;
        services.AddSingleton(commandType, commandType);
    }

    public void RegisterDefault<TCommand>() where TCommand: IScriptCommand
    {
        var commandType = typeof(TCommand);
        defaultCommandType = commandType;
        services.AddSingleton(commandType, commandType);
    }

    public Type GetCommandType(string commandName)
    {
        if(commands.ContainsKey(commandName))
        {
            return commands[commandName];
        }

        return null;
    }
}