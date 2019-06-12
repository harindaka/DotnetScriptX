#load "IScriptCommand.csx"

public interface IScriptCommandCollection
{
    Func<string, Task<Type>> OnCommandNotFoundAsync { get; set; }
    void Register<TCommand>(string commandName) where TCommand: IScriptCommand;
    void RegisterDefault<TCommand>() where TCommand: IScriptCommand;
}