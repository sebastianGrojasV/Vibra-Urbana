param(
    [string]$AzureConnection = $env:VIBRA_AZURE_CONNECTION,

    [string]$DataScript = ".database\local-data.sql",

    [switch]$SkipData
)

$ErrorActionPreference = "Stop"

if ([string]::IsNullOrWhiteSpace($AzureConnection)) {
    dotnet ef database update
}
else {
    dotnet ef database update --connection "$AzureConnection"
}

if (-not $SkipData) {
    if (-not (Test-Path -LiteralPath $DataScript)) {
        throw "Data script not found: $DataScript. Run Scripts\Export-LocalData.ps1 first or pass -SkipData."
    }

    if ([string]::IsNullOrWhiteSpace($AzureConnection)) {
        throw "Set VIBRA_AZURE_CONNECTION or pass -AzureConnection to load data."
    }

    $script = Get-Content -Raw -LiteralPath $DataScript
    $connection = [System.Data.SqlClient.SqlConnection]::new($AzureConnection)
    $connection.Open()

    try {
        $command = $connection.CreateCommand()
        $command.CommandTimeout = 0
        $command.CommandText = $script
        [void]$command.ExecuteNonQuery()
    }
    finally {
        $connection.Dispose()
    }
}
