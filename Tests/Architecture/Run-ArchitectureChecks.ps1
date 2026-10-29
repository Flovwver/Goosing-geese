$ErrorActionPreference = 'Stop'
$projectRoot = [IO.Path]::GetFullPath((Join-Path $PSScriptRoot '../..'))
$previousLocation = Get-Location

function Invoke-UnityCommand {
    param([string[]] $CommandArguments)
    $json = (& unity command @CommandArguments --project-path $projectRoot --json | Out-String)

    if ($LASTEXITCODE -ne 0) {
        throw "Unity command failed: $json"
    }

    $response = $json | ConvertFrom-Json

    if ($response.success -eq $false -or $response.data.result.success -eq $false) {
        throw "Unity check failed: $json"
    }

    return $response.data.result
}

try {
    Set-Location -LiteralPath $projectRoot

    if (-not $env:DOTNET_CLI_HOME) {
        $env:DOTNET_CLI_HOME = Join-Path $projectRoot 'Temp/dotnet-home'
    }

    & dotnet run --project Tools/CodeStyle -- --check

    if ($LASTEXITCODE -ne 0) {
        throw 'C# style checks failed.'
    }

    & python Tests/Architecture/build_suite.py

    if ($LASTEXITCODE -ne 0) {
        throw 'Could not generate the architecture suite.'
    }

    $status = Invoke-UnityCommand -CommandArguments @('editor_status')
    $wasPlaying = $status.playMode -eq 'playing'
    $backedUp = $false
    $enteredPlay = $false

    try {
        Invoke-UnityCommand -CommandArguments @('run_script', 'Tests/Architecture/PreferencesBackup.cs', 'PreferencesBackup.Capture') | Out-Null
        $backedUp = $true

        if ($wasPlaying -eq $false) {
            Invoke-UnityCommand -CommandArguments @('editor_play') | Out-Null
            $enteredPlay = $true
        }

        $result = Invoke-UnityCommand -CommandArguments @('run_script', 'Temp/ArchitectureSuite.cs', 'ArchitectureSuite.Main')
        Write-Output $result.result
    }
    finally {
        try {
            if ($enteredPlay) {
                Invoke-UnityCommand -CommandArguments @('editor_stop') | Out-Null
            }
        }
        finally {
            if ($backedUp) {
                Invoke-UnityCommand -CommandArguments @('run_script', 'Tests/Architecture/PreferencesBackup.cs', 'PreferencesBackup.Restore') | Out-Null
            }
        }
    }
}
finally {
    Set-Location -LiteralPath $previousLocation.Path
}
