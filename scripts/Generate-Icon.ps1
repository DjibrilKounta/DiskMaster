# Construit une icône géométrique originale, sans reprendre les images historiques.
[CmdletBinding()]
param()
$ErrorActionPreference = 'Stop'
Add-Type -AssemblyName System.Drawing
$repoRoot = Split-Path $PSScriptRoot -Parent
$target = Join-Path $repoRoot 'DiskMaster/Assets/DiskMaster.ico'
$images = @()
foreach ($size in @(16, 32, 48, 64, 128, 256)) {
    $bitmap = [Drawing.Bitmap]::new($size, $size)
    $canvas = [Drawing.Graphics]::FromImage($bitmap)
    $canvas.SmoothingMode = [Drawing.Drawing2D.SmoothingMode]::AntiAlias
    $canvas.ScaleTransform(($size / 256.0), ($size / 256.0))
    $accent = [Drawing.SolidBrush]::new([Drawing.ColorTranslator]::FromHtml('#007F70'))
    $white = [Drawing.SolidBrush]::new([Drawing.Color]::White)
    $pale = [Drawing.SolidBrush]::new([Drawing.ColorTranslator]::FromHtml('#D9F2ED'))
    $pen = [Drawing.Pen]::new($accent, 12)
    $pen.StartCap = [Drawing.Drawing2D.LineCap]::Round
    $pen.EndCap = [Drawing.Drawing2D.LineCap]::Round
    $stream = [IO.MemoryStream]::new()
    try {
        $canvas.Clear([Drawing.Color]::Transparent)
        $canvas.FillRectangle($accent, 8, 8, 240, 240)
        $canvas.FillRectangle($white, 48, 53, 160, 150)
        $canvas.FillEllipse($pale, 88, 76, 80, 80)
        $canvas.FillEllipse($accent, 115, 103, 26, 26)
        $canvas.DrawLine($pen, 128, 116, 170, 157)
        $canvas.FillEllipse($accent, 169, 173, 14, 14)
        $canvas.DrawLine($pen, 73, 180, 112, 180)
        $bitmap.Save($stream, [Drawing.Imaging.ImageFormat]::Png)
        $images += ,@($size, $stream.ToArray())
    }
    finally {
        $stream.Dispose(); $pen.Dispose(); $accent.Dispose(); $white.Dispose(); $pale.Dispose()
        $canvas.Dispose(); $bitmap.Dispose()
    }
}
$file = [IO.File]::Create($target)
$writer = [IO.BinaryWriter]::new($file)
try {
    $writer.Write([uint16]0); $writer.Write([uint16]1); $writer.Write([uint16]$images.Count)
    $offset = 6 + 16 * $images.Count
    foreach ($item in $images) {
        $dimension = if ($item[0] -eq 256) { 0 } else { $item[0] }
        $writer.Write([byte]$dimension); $writer.Write([byte]$dimension)
        $writer.Write([byte]0); $writer.Write([byte]0)
        $writer.Write([uint16]1); $writer.Write([uint16]32)
        $writer.Write([uint32]$item[1].Length); $writer.Write([uint32]$offset)
        $offset += $item[1].Length
    }
    foreach ($item in $images) { $writer.Write([byte[]]$item[1]) }
}
finally { $writer.Dispose(); $file.Dispose() }
Write-Host "Icône générée : $target"
