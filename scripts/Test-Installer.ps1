[CmdletBinding()]
param([Parameter(Mandatory = $true)][string]$InstallerPath)
$ErrorActionPreference = 'Stop'
Set-StrictMode -Version Latest
$repoRoot = [IO.Path]::GetFullPath((Split-Path $PSScriptRoot -Parent))
$installer = (Resolve-Path -LiteralPath $InstallerPath).Path
$appKey = 'Software\Microsoft\Windows\CurrentVersion\Uninstall\{70DE9D7B-87C6-4CBD-9DC6-F09F76917E5C}_is1'
# Ne jamais remplacer une installation utilisateur existante pour effectuer un test.
foreach ($view in @([Microsoft.Win32.RegistryView]::Registry32, [Microsoft.Win32.RegistryView]::Registry64)) {
    $base = [Microsoft.Win32.RegistryKey]::OpenBaseKey([Microsoft.Win32.RegistryHive]::CurrentUser, $view)
    try {
        $existing = $base.OpenSubKey($appKey)
        if ($existing) { $existing.Dispose(); throw 'DiskMaster est déjà installé : effectuer ce test dans une VM propre.' }
    }
    finally { $base.Dispose() }
}
$testRoot = Join-Path $repoRoot ('artifacts/installer-tests/' + [guid]::NewGuid().ToString('N'))
$installRoot = Join-Path $testRoot 'installed'
New-Item -ItemType Directory -Path $testRoot -Force | Out-Null
$marker = Join-Path $testRoot 'keep-outside-installation.txt'
'Ne pas supprimer' | Set-Content -LiteralPath $marker -Encoding UTF8
$setupLog = Join-Path $testRoot 'setup.log'
$arguments = @('/VERYSILENT', '/SUPPRESSMSGBOXES', '/NORESTART', '/CURRENTUSER', '/NOICONS',
    ('/DIR="' + $installRoot + '"'), ('/LOG="' + $setupLog + '"'))
$process = Start-Process -FilePath $installer -ArgumentList $arguments -WindowStyle Hidden -Wait -PassThru
if ($process.ExitCode -ne 0) { throw "Installation échouée : $($process.ExitCode). Journal : $setupLog" }
$installedExe = Join-Path $installRoot 'DiskMaster.exe'
if (-not (Test-Path -LiteralPath $installedExe -PathType Leaf)) { throw 'Exécutable installé absent.' }
if (-not (Test-Path -LiteralPath (Join-Path $installRoot 'coreclr.dll'))) { throw 'Runtime embarqué absent.' }
if (-not (Test-Path -LiteralPath (Join-Path $installRoot 'docs/USER-GUIDE.md'))) { throw 'Guide installé absent.' }

# Vérifier le périmètre physique avant de lancer le désinstalleur qui retire ses propres fichiers.
$resolvedInstallRoot = (Resolve-Path -LiteralPath $installRoot).Path
if ($resolvedInstallRoot -ne [IO.Path]::GetFullPath($installRoot) -or
    -not $resolvedInstallRoot.StartsWith($repoRoot + [IO.Path]::DirectorySeparatorChar, [StringComparison]::OrdinalIgnoreCase)) {
    throw 'Périmètre de désinstallation invalide.'
}
for ($ancestor = $resolvedInstallRoot; $ancestor; $ancestor = [IO.Path]::GetDirectoryName($ancestor)) {
    if (((Get-Item -LiteralPath $ancestor -Force).Attributes -band [IO.FileAttributes]::ReparsePoint) -ne 0) {
        throw 'Jonction dans le chemin de test : désinstallation automatique refusée.'
    }
}
$uninstaller = Join-Path $resolvedInstallRoot 'unins000.exe'
if (-not (Test-Path -LiteralPath $uninstaller -PathType Leaf)) { throw 'Désinstalleur absent.' }
$uninstallLog = Join-Path $testRoot 'uninstall.log'
$uninstall = Start-Process -FilePath $uninstaller -ArgumentList @('/VERYSILENT', '/SUPPRESSMSGBOXES', '/NORESTART',
    ('/LOG="' + $uninstallLog + '"')) -WindowStyle Hidden -Wait -PassThru
if ($uninstall.ExitCode -ne 0) { throw "Désinstallation échouée : $($uninstall.ExitCode)" }
if (Test-Path -LiteralPath $installedExe) { throw 'Exécutable encore présent après désinstallation.' }
if (-not (Test-Path -LiteralPath $marker)) { throw 'Le témoin extérieur a été supprimé.' }
Write-Host "Installation et désinstallation locales réussies. Journaux : $testRoot"
Write-Host 'Ce test ne remplace pas les essais de mise à niveau et de VM sans runtime.'
