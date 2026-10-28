[CmdletBinding()]
param(
    [switch]$WithInstaller,
    [string]$IsccPath,
    [string]$CertificateThumbprint,
    [string]$SignToolPath,
    [string]$TimestampUrl = 'https://timestamp.digicert.com'
)

$ErrorActionPreference = 'Stop'
Set-StrictMode -Version Latest
$repoRoot = Split-Path $PSScriptRoot -Parent
[xml]$project = Get-Content -LiteralPath (Join-Path $repoRoot 'DiskMaster/DiskMaster.csproj')
$appVersion = [string]$project.Project.PropertyGroup.Version
if ($appVersion -notmatch '^\d+\.\d+\.\d+(-[a-zA-Z0-9.-]+)?$') { throw 'Version de projet invalide.' }

# Aucune suppression de sortie : chaque fabrication emploie son propre dossier.
$runId = (Get-Date -Format 'yyyyMMdd-HHmmss') + '-' + [guid]::NewGuid().ToString('N').Substring(0, 8)
$outputRoot = Join-Path $repoRoot "artifacts/releases/$appVersion/$runId"
$publishRoot = Join-Path $outputRoot 'app'
$buildRoot = Join-Path $outputRoot 'build'
New-Item -ItemType Directory -Path $outputRoot -Force | Out-Null

if ($WithInstaller) {
    if (-not $IsccPath) {
        $compiler = Get-Command ISCC.exe -ErrorAction SilentlyContinue
        if ($compiler) { $IsccPath = $compiler.Source }
        else { $IsccPath = Join-Path ${env:ProgramFiles(x86)} 'Inno Setup 6/ISCC.exe' }
    }
    if (-not (Test-Path -LiteralPath $IsccPath -PathType Leaf)) {
        throw 'ISCC.exe absent. Installez Inno Setup ou fournissez -IsccPath. Aucune installation automatique.'
    }
}
if ($CertificateThumbprint) {
    if ($CertificateThumbprint -notmatch '^[0-9a-fA-F]{40}$') { throw 'Empreinte de certificat invalide.' }
    if (-not $SignToolPath -or -not (Test-Path -LiteralPath $SignToolPath -PathType Leaf)) {
        throw 'Fournissez -SignToolPath vers le SignTool du SDK Windows.'
    }
    if ($TimestampUrl -notmatch '^https://[a-zA-Z0-9./:-]+$') { throw 'Serveur d''horodatage HTTPS invalide.' }
}

Push-Location $repoRoot
try {
    # Une erreur de test interrompt toujours la fabrication.
    & (Join-Path $PSScriptRoot 'Test.ps1')
    & dotnet publish DiskMaster/DiskMaster.csproj -c Release -r win-x64 --self-contained true `
        -o $publishRoot '-p:PublishTrimmed=false' '-p:PublishSingleFile=false' `
        '-p:CopyOutputSymbolsToPublishDirectory=false' '-p:UseArtifactsOutput=true' "-p:ArtifactsPath=$buildRoot" `
        "-p:NuGetLockFilePath=$buildRoot/publish.packages.lock.json"
    if ($LASTEXITCODE -ne 0) { throw 'Publication autonome échouée.' }
    Copy-Item -LiteralPath (Join-Path $buildRoot 'publish.packages.lock.json') -Destination (Join-Path $publishRoot 'release-runtime-lock.json')
    & (Join-Path $PSScriptRoot 'Copy-RuntimeNotices.ps1') -PublishDirectory $publishRoot -BuildDirectory $buildRoot
    foreach ($file in @('LICENSE', 'README.md', 'CHANGELOG.md', 'CONTRIBUTING.md', 'PRIVACY.md', 'THIRD-PARTY-NOTICES.md', 'SECURITY.md', 'SUPPORT.md')) {
        Copy-Item -LiteralPath (Join-Path $repoRoot $file) -Destination $publishRoot
    }
    Copy-Item -LiteralPath (Join-Path $repoRoot 'docs') -Destination (Join-Path $publishRoot 'docs') -Recurse
    & (Join-Path $PSScriptRoot 'Check-Documentation.ps1') -RootDirectory $publishRoot
    $appExe = Join-Path $publishRoot 'DiskMaster.exe'
    if ($CertificateThumbprint) {
        & $SignToolPath sign /sha1 $CertificateThumbprint /fd SHA256 /tr $TimestampUrl /td SHA256 $appExe
        if ($LASTEXITCODE -ne 0) { throw 'Signature de l''application échouée.' }
        & $SignToolPath verify /pa $appExe
        if ($LASTEXITCODE -ne 0) { throw 'Vérification de signature échouée.' }
    }
    $sdkVersion = (& dotnet --version).Trim()
    $commit = $null
    if (Test-Path -LiteralPath (Join-Path $repoRoot '.git')) {
        $commit = & git rev-parse HEAD
        if ($LASTEXITCODE -ne 0) { throw 'Impossible de déterminer le commit.' }
        $changes = & git status --porcelain
        if ($changes) { throw 'Le dépôt doit être propre pour fabriquer une release traçable.' }
    }
    $manifest = [ordered]@{
        product = 'DiskMaster'; version = $appVersion; channel = 'alpha'; runtime = 'win-x64'
        selfContained = $true; sdk = $sdkVersion; commit = $commit
        builtAtUtc = [DateTime]::UtcNow.ToString('O'); authenticodeSigned = [bool]$CertificateThumbprint
        qualification = 'Voir docs/RELEASE-CHECKLIST.md : qualification manuelle requise.'
    }
    $manifestPath = Join-Path $publishRoot 'release-manifest.json'
    $manifest | ConvertTo-Json | Set-Content -LiteralPath $manifestPath -Encoding UTF8
    $zipPath = Join-Path $outputRoot "DiskMaster-$appVersion-win-x64.zip"
    Compress-Archive -Path (Join-Path $publishRoot '*') -DestinationPath $zipPath -CompressionLevel Optimal
    $deliverables = @($zipPath)
    if ($WithInstaller) {
        $compilerArgs = @('/Q', "/DAppVersion=$appVersion", "/DPublishDir=$publishRoot", "/DOutputDir=$outputRoot")
        if ($CertificateThumbprint) {
            $signCommand = '$q' + $SignToolPath + '$q sign /sha1 ' + $CertificateThumbprint + ' /fd SHA256 /tr ' + $TimestampUrl + ' /td SHA256 $f'
            $compilerArgs += '/DSigningEnabled'
            $compilerArgs += "/SdiskmasterSign=$signCommand"
        }
        & $IsccPath @compilerArgs (Join-Path $repoRoot 'installer/DiskMaster.iss')
        if ($LASTEXITCODE -ne 0) { throw 'Compilation de l''installateur échouée.' }
        $setup = Join-Path $outputRoot "DiskMaster-$appVersion-Setup.exe"
        if (-not (Test-Path -LiteralPath $setup)) { throw 'Installateur non produit.' }
        if ($CertificateThumbprint) {
            & $SignToolPath verify /pa $setup
            if ($LASTEXITCODE -ne 0) { throw 'Signature de l''installateur invalide.' }
        }
        $deliverables += $setup
    }
    $lines = foreach ($file in $deliverables) {
        $hash = Get-FileHash -LiteralPath $file -Algorithm SHA256
        "$($hash.Hash.ToLowerInvariant())  $([IO.Path]::GetFileName($file))"
    }
    $lines | Set-Content -LiteralPath (Join-Path $outputRoot 'SHA256SUMS.txt') -Encoding ASCII
    & (Join-Path $PSScriptRoot 'Verify-Package.ps1') -PackageDirectory $outputRoot
    Write-Host "Release locale préparée : $outputRoot"
    Write-Host 'Aucun fichier envoyé ni publié. Signature et qualification : voir le manifeste.'
}
finally { Pop-Location }
