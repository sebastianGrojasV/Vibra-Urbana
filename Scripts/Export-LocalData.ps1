param(
    [string]$SourceConnection = "Server=localhost\SQLEXPRESS;Database=VibraUrbanaDb;Trusted_Connection=True;TrustServerCertificate=True;",
    [string]$OutputPath = ".database\local-data.sql"
)

$ErrorActionPreference = "Stop"

$tables = @(
    "Roles",
    "Permisos",
    "RolPermisos",
    "MetodosPago",
    "Usuarios",
    "Clientes",
    "Categorias",
    "Productos",
    "Inventario",
    "MovimientosInventario",
    "Ventas",
    "DetalleVentas",
    "Facturas",
    "CierresCaja",
    "Pedidos",
    "DetallePedidos",
    "ComprobantesPago",
    "Bitacora"
)

function Quote-Name([string]$name) {
    return "[" + $name.Replace("]", "]]") + "]"
}

function Format-SqlValue($value, [string]$dataType) {
    if ($null -eq $value -or $value -is [DBNull]) {
        return "NULL"
    }

    switch -Regex ($dataType) {
        "^(bit)$" {
            if ([bool]$value) { return "1" }
            return "0"
        }
        "^(tinyint|smallint|int|bigint|decimal|numeric|money|smallmoney|float|real)$" {
            return ([string]$value).Replace(",", ".")
        }
        "^(datetime|datetime2|smalldatetime|date|time|datetimeoffset)$" {
            return "N'" + ([DateTime]$value).ToString("yyyy-MM-ddTHH:mm:ss.fffffff") + "'"
        }
        "^(uniqueidentifier)$" {
            return "'" + $value.ToString() + "'"
        }
        "^(varbinary|binary|image)$" {
            $bytes = [byte[]]$value
            return "0x" + (($bytes | ForEach-Object { $_.ToString("X2") }) -join "")
        }
        default {
            return "N'" + ([string]$value).Replace("'", "''") + "'"
        }
    }
}

$outputDirectory = Split-Path -Parent $OutputPath
if ($outputDirectory -and -not (Test-Path $outputDirectory)) {
    New-Item -ItemType Directory -Path $outputDirectory | Out-Null
}

$connection = [System.Data.SqlClient.SqlConnection]::new($SourceConnection)
$connection.Open()

try {
    $writer = [System.IO.StreamWriter]::new((Resolve-Path -LiteralPath $outputDirectory).Path + "\" + (Split-Path -Leaf $OutputPath), $false, [System.Text.UTF8Encoding]::new($false))

    try {
        $writer.WriteLine("SET NOCOUNT ON;")
        $writer.WriteLine("SET XACT_ABORT ON;")
        $writer.WriteLine("BEGIN TRANSACTION;")
        $writer.WriteLine("")

        foreach ($table in $tables) {
            $writer.WriteLine("ALTER TABLE dbo.$(Quote-Name $table) NOCHECK CONSTRAINT ALL;")
        }

        $writer.WriteLine("")

        for ($index = $tables.Count - 1; $index -ge 0; $index--) {
            $table = $tables[$index]
            $writer.WriteLine("DELETE FROM dbo.$(Quote-Name $table);")
        }

        $writer.WriteLine("")

        foreach ($table in $tables) {
            $metadataCommand = $connection.CreateCommand()
            $metadataCommand.CommandText = @"
SELECT
    c.name AS ColumnName,
    ty.name AS DataType,
    c.is_identity AS IsIdentity
FROM sys.columns c
JOIN sys.types ty ON c.user_type_id = ty.user_type_id
JOIN sys.tables t ON c.object_id = t.object_id
WHERE t.name = @TableName
  AND SCHEMA_NAME(t.schema_id) = 'dbo'
  AND c.is_computed = 0
  AND ty.name NOT IN ('timestamp', 'rowversion')
ORDER BY c.column_id;
"@
            [void]$metadataCommand.Parameters.AddWithValue("@TableName", $table)

            $metadata = New-Object System.Data.DataTable
            $metadata.Load($metadataCommand.ExecuteReader())

            if ($metadata.Rows.Count -eq 0) {
                continue
            }

            $columns = @($metadata.Rows | ForEach-Object { $_.ColumnName })
            $columnList = ($columns | ForEach-Object { Quote-Name $_ }) -join ", "
            $hasIdentity = @($metadata.Rows | Where-Object { $_.IsIdentity -eq $true -or $_.IsIdentity -eq 1 }).Count -gt 0

            $dataCommand = $connection.CreateCommand()
            $dataCommand.CommandText = "SELECT $columnList FROM dbo.$(Quote-Name $table);"
            $reader = $dataCommand.ExecuteReader()

            $rows = New-Object System.Data.DataTable
            $rows.Load($reader)

            if ($rows.Rows.Count -eq 0) {
                continue
            }

            $writer.WriteLine("-- dbo.$table")

            if ($hasIdentity) {
                $writer.WriteLine("SET IDENTITY_INSERT dbo.$(Quote-Name $table) ON;")
            }

            foreach ($row in $rows.Rows) {
                $values = foreach ($column in $metadata.Rows) {
                    Format-SqlValue $row[$column.ColumnName] $column.DataType
                }

                $writer.WriteLine("INSERT INTO dbo.$(Quote-Name $table) ($columnList) VALUES ($($values -join ', '));")
            }

            if ($hasIdentity) {
                $writer.WriteLine("SET IDENTITY_INSERT dbo.$(Quote-Name $table) OFF;")
            }

            $writer.WriteLine("")
        }

        foreach ($table in $tables) {
            $writer.WriteLine("ALTER TABLE dbo.$(Quote-Name $table) WITH CHECK CHECK CONSTRAINT ALL;")
        }

        $writer.WriteLine("")
        $writer.WriteLine("COMMIT TRANSACTION;")
    }
    finally {
        $writer.Dispose()
    }
}
finally {
    $connection.Dispose()
}

Write-Host "Data script generated at $OutputPath"
