# Download data script for Windows

$DataDir = $PSScriptRoot
$ProjectRoot = Split-Path $PSScriptRoot -Parent

$Python = Join-Path $ProjectRoot ".venv/Scripts/python.exe"
$Script = Join-Path $PSScriptRoot "filtercsv.py"

$Urls = @(
    "https://data.open-power-system-data.org/time_series/2020-10-06/time_series_15min_singleindex.csv",
    "https://data.open-power-system-data.org/time_series/2020-10-06/time_series_60min_singleindex.csv"
)

foreach ($Url in $Urls) {
    $FileName = Split-Path $Url -Leaf

    $OutputPath = Join-Path $DataDir $FileName

    Write-Host "Downloading $Url -> $OutputPath ..."
    curl.exe -L -o $OutputPath $Url
    Write-Host "✅ Done.`n"
}

& $Python $Script