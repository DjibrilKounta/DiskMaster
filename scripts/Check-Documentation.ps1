[CmdletBinding()]
param([string]$RootDirectory)
$ErrorActionPreference = 'Stop'
$repoRoot = if ($RootDirectory) { (Resolve-Path -LiteralPath $RootDirectory).Path } else { Split-Path $PSScriptRoot -Parent }
$documents = @(Get-ChildItem -LiteralPath $repoRoot -Filter '*.md' -File)
$documents += @(Get-ChildItem -LiteralPath (Join-Path $repoRoot 'docs') -Filter '*.md' -File)
foreach ($document in $documents) {
    $content = [IO.File]::ReadAllText($document.FullName, [Text.Encoding]::UTF8)
    foreach ($link in [regex]::Matches($content, '\]\(([^)]+)\)')) {
        $target = $link.Groups[1].Value
        if ($target -match '^(https?://|#)') { continue }
        $relative = ($target -split '#')[0]
        $resolved = Join-Path $document.DirectoryName $relative
        if (-not (Test-Path -LiteralPath $resolved)) { throw "Lien local absent : $($document.Name) -> $target" }
    }
}
foreach ($script in Get-ChildItem -LiteralPath $PSScriptRoot -Filter '*.ps1' -File) {
    $parseTokens = $null; $parseErrors = $null
    [void][Management.Automation.Language.Parser]::ParseFile($script.FullName, [ref]$parseTokens, [ref]$parseErrors)
    if ($parseErrors.Count -gt 0) { throw "Syntaxe PowerShell invalide : $($script.Name) : $parseErrors" }
}
Write-Host "$($documents.Count) documents : liens locaux et syntaxe des scripts vérifiés."
