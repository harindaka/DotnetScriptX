using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

public interface IExecutionContext
{
    string ScriptEnvironment { get; }
    bool IsOSPlatform(OSPlatform platform);
    string GetScriptFilePath([CallerFilePath] string file = "");
    string GetScriptDirectoryPath([CallerFilePath] string file = "");
}