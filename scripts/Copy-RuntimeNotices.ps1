[CmdletBinding()]
param(
    [Parameter(Mandatory = $true)][string]$PublishDirectory,
    [Parameter(Mandatory = $true)][string]$BuildDirectory
)
$ErrorActionPreference = 'Stop'
$deps = Get-Content -LiteralPath (Join-Path $PublishDirectory 'DiskMaster.deps.json') -Raw | ConvertFrom-Json
$assetsFile = Get-ChildItem -LiteralPath $BuildDirectory -Filter project.assets.json -Recurse -File | Select-Object -First 1
if (-not $assetsFile) { throw 'Métadonnées de restauration absentes.' }
$assets = Get-Content -LiteralPath $assetsFile.FullName -Raw | ConvertFrom-Json
$packageRoots = @($assets.packageFolders.PSObject.Properties.Name)
$runtimeLibraries = @($deps.libraries.PSObject.Properties | Where-Object { $_.Name -like 'runtimepack.*' })
if ($runtimeLibraries.Count -lt 2) { throw 'Les runtimes Core et Desktop sont attendus.' }
foreach ($library in $runtimeLibraries) {
    $parts = $library.Name.Substring('runtimepack.'.Length).Split('/')
    $id = $parts[0].ToLowerInvariant(); $version = $parts[1]
    $package = $null
    foreach ($root in $packageRoots) {
        $candidate = Join-Path $root "$id/$version"
        if (Test-Path -LiteralPath $candidate -PathType Container) { $package = $candidate; break }
    }
    if (-not $package) { throw "Runtime source introuvable : $id/$version" }
    $licenseFiles = @(Get-ChildItem -LiteralPath $package -File | Where-Object { $_.Name -match '^(LICENSE|THIRD-PARTY-NOTICES)(\.TXT)?$' })
    if (-not @($licenseFiles | Where-Object { $_.Name -like 'LICENSE*' }).Count) { throw "Licence runtime absente : $id" }
    $target = Join-Path $PublishDirectory "licenses/$id"
    New-Item -ItemType Directory -Path $target -Force | Out-Null
    foreach ($file in $licenseFiles) { Copy-Item -LiteralPath $file.FullName -Destination $target }
    "$id/$version" | Set-Content -LiteralPath (Join-Path $target 'VERSION.txt') -Encoding ASCII
}
