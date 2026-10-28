[CmdletBinding()]
param([Parameter(Mandatory = $true)][string]$PackageDirectory)

$ErrorActionPreference = 'Stop'
Set-StrictMode -Version Latest
$packageRoot = (Resolve-Path -LiteralPath $PackageDirectory).Path
$checksumFile = Join-Path $packageRoot 'SHA256SUMS.txt'
if (-not (Test-Path -LiteralPath $checksumFile -PathType Leaf)) { throw 'Liste SHA256 absente.' }
$names = @()
foreach ($line in Get-Content -LiteralPath $checksumFile) {
    if ($line -notmatch '^([a-f0-9]{64})  ([a-zA-Z0-9._-]+)$') { throw 'Entrée SHA256 invalide.' }
    $expected = $Matches[1]; $name = $Matches[2]
    if ($names -contains $name) { throw 'Entrée SHA256 dupliquée.' }
    $names += $name
    $actual = (Get-FileHash -LiteralPath (Join-Path $packageRoot $name) -Algorithm SHA256).Hash
    if ($actual -ne $expected) { throw "Empreinte incorrecte : $name" }
}
$zips = @($names | Where-Object { $_ -like '*.zip' })
if ($zips.Count -ne 1) { throw 'Une archive ZIP est attendue.' }
Add-Type -AssemblyName System.IO.Compression.FileSystem
$archive = [IO.Compression.ZipFile]::OpenRead((Join-Path $packageRoot $zips[0]))
try {
    $entryNames = @($archive.Entries | ForEach-Object { $_.FullName.Replace('\', '/') })
    foreach ($name in $entryNames) {
        if ($name.StartsWith('/') -or $name -match '(^|/)\.\.(/|$)' -or $name.Contains(':')) {
            throw "Chemin d''archive interdit : $name"
        }
    }
    foreach ($required in @('DiskMaster.exe', 'DiskMaster.dll', 'coreclr.dll', 'PresentationFramework.dll',
        'LICENSE', 'README.md', 'CHANGELOG.md', 'PRIVACY.md', 'release-manifest.json', 'docs/USER-GUIDE.md',
        'licenses/microsoft.netcore.app.runtime.win-x64/LICENSE.TXT',
        'licenses/microsoft.netcore.app.runtime.win-x64/THIRD-PARTY-NOTICES.TXT',
        'licenses/microsoft.windowsdesktop.app.runtime.win-x64/LICENSE')) {
        if ($entryNames -notcontains $required) { throw "Fichier manquant : $required" }
    }
    $manifestEntry = $archive.GetEntry('release-manifest.json')
    $reader = [IO.StreamReader]::new($manifestEntry.Open())
    try { $manifest = $reader.ReadToEnd() | ConvertFrom-Json }
    finally { $reader.Dispose() }
    if (-not $manifest.selfContained -or $manifest.runtime -ne 'win-x64') { throw 'Manifeste incompatible.' }
    $exe = $archive.GetEntry('DiskMaster.exe').Open()
    $buffer = [IO.MemoryStream]::new()
    try {
        $exe.CopyTo($buffer)
        $bytes = $buffer.ToArray()
        if ($bytes.Length -lt 64 -or [BitConverter]::ToUInt16($bytes, 0) -ne 0x5A4D) { throw 'Exécutable PE invalide.' }
        $offset = [BitConverter]::ToInt32($bytes, 0x3C)
        if ($offset -lt 0 -or $offset + 6 -gt $bytes.Length -or [BitConverter]::ToUInt32($bytes, $offset) -ne 0x4550 -or
            [BitConverter]::ToUInt16($bytes, $offset + 4) -ne 0x8664) { throw 'Exécutable x64 invalide.' }
    }
    finally { $exe.Dispose(); $buffer.Dispose() }
}
finally { $archive.Dispose() }
Write-Host 'Archive, fichiers requis, architecture x64 et empreintes SHA256 vérifiés.'
Write-Host 'Une empreinte vérifie l''intégrité ; elle ne prouve pas l''identité de l''éditeur.'
