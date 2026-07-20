param(
    [string]$LaunchProfile = "http",
    [switch]$UsePollingWatcher
)

$ErrorActionPreference = "Stop"

$projectPath = Join-Path $PSScriptRoot "..\VibraUrbana.csproj"

$env:ASPNETCORE_ENVIRONMENT = "Development"
$env:DOTNET_WATCH_SUPPRESS_BROWSER_REFRESH = "0"

if ($UsePollingWatcher) {
    $env:DOTNET_USE_POLLING_FILE_WATCHER = "1"
}

dotnet watch --project $projectPath --launch-profile $LaunchProfile run
