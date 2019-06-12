public interface IScriptCommand
{
    Task ExecuteAsync(string[] args);
}