#load "IExecutionContext.csx"

using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.IO;

public class ExecutionContext: IExecutionContext
{
    public string ScriptEnvironment { get; private set; }

    public bool IsOSPlatform(OSPlatform platform)
    {
        return RuntimeInformation.IsOSPlatform(platform);
    }

    public ExecutionContext(string environment)
    {
        this.ScriptEnvironment = environment;
    }

    public string GetScriptFilePath([CallerFilePath] string file = "") 
    { 
        return file;
    }

    public string GetScriptDirectoryPath([CallerFilePath] string file = "") 
    { 
        return Path.GetDirectoryName(file);
    }
}