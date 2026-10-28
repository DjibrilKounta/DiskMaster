[CmdletBinding()]
param()

$ErrorActionPreference = 'Stop'
Set-StrictMode -Version Latest
$repoRoot = Split-Path $PSScriptRoot -Parent
$artifactRoot = Join-Path $repoRoot ('artifacts/checks/' + [guid]::NewGuid().ToString('N'))
Push-Location $repoRoot
try {
    & (Join-Path $PSScriptRoot 'Check-Documentation.ps1')
    & dotnet restore DiskMaster.slnx --locked-mode "-p:UseArtifactsOutput=true" "-p:ArtifactsPath=$artifactRoot"
    if ($LASTEXITCODE -ne 0) { throw 'Restauration verrouillée échouée.' }
    & dotnet build DiskMaster.slnx -c Release --no-restore "-p:UseArtifactsOutput=true" "-p:ArtifactsPath=$artifactRoot"
    if ($LASTEXITCODE -ne 0) { throw 'Compilation échouée.' }
    $testAssembly = Join-Path $artifactRoot 'bin/DiskMaster.Tests/release/DiskMaster.Tests.dll'
    & dotnet $testAssembly
    if ($LASTEXITCODE -ne 0) { throw 'Tests échoués.' }
    Write-Host "Validation réussie. Sorties : $artifactRoot"
}
finally { Pop-Location }
