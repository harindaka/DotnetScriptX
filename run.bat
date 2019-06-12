@echo off

WHERE dotnet >nul 2>nul
if %ERRORLEVEL% NEQ 0 (
    echo The .Net Runtime is required to run this script. Please install it and try again...
    exit /b 1
)

WHERE dotnet-script >nul 2>nul
if %ERRORLEVEL% NEQ 0 (
    echo Installing dotnet-script tool... 
    dotnet tool install -g dotnet-script
    echo(
)

dotnet script "%~dp0dsx\Program.csx" %*