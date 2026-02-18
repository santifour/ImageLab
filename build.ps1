# ImageToolkit Build Script
# Bu script uygulamayı derler ve masaüstüne kopyalar

Write-Host "==================================" -ForegroundColor Cyan
Write-Host "  ImageToolkit Build Script" -ForegroundColor Cyan
Write-Host "==================================" -ForegroundColor Cyan
Write-Host ""

# Clean previous builds
Write-Host "[1/4] Cleaning previous builds..." -ForegroundColor Yellow
Remove-Item -Path "bin\Release" -Recurse -Force -ErrorAction SilentlyContinue
Write-Host "      Done!" -ForegroundColor Green
Write-Host ""

# Build
Write-Host "[2/4] Building application..." -ForegroundColor Yellow
Write-Host "      Mode: Release" -ForegroundColor Gray
Write-Host "      Platform: Windows x64" -ForegroundColor Gray
Write-Host "      Type: Self-contained single file" -ForegroundColor Gray
Write-Host ""

dotnet publish -c Release -r win-x64 --self-contained true -p:PublishSingleFile=true -p:IncludeNativeLibrariesForSelfExtract=true -p:EnableCompressionInSingleFile=true

if ($LASTEXITCODE -ne 0) {
    Write-Host ""
    Write-Host "Build failed!" -ForegroundColor Red
    exit 1
}

Write-Host ""
Write-Host "      Build successful!" -ForegroundColor Green
Write-Host ""

# Check file
Write-Host "[3/4] Checking output..." -ForegroundColor Yellow
$exePath = "bin\Release\net8.0-windows\win-x64\publish\ImageLab.exe"

if (Test-Path $exePath) {
    $fileSize = (Get-Item $exePath).Length / 1MB
    Write-Host "      File: ImageLab.exe" -ForegroundColor Gray
    Write-Host "      Size: $([math]::Round($fileSize, 2)) MB" -ForegroundColor Gray
    Write-Host "      Done!" -ForegroundColor Green
}
else {
    Write-Host "      Error: EXE file not found!" -ForegroundColor Red
    exit 1
}
Write-Host ""

# Copy to desktop
Write-Host "[4/4] Copying to Desktop..." -ForegroundColor Yellow
$desktopPath = [Environment]::GetFolderPath("Desktop")
$destPath = "$desktopPath\ImageLab.exe"

Copy-Item $exePath -Destination $destPath -Force

if (Test-Path $destPath) {
    Write-Host "      Copied to: $destPath" -ForegroundColor Gray
    Write-Host "      Done!" -ForegroundColor Green
}
else {
    Write-Host "      Error: Failed to copy to Desktop!" -ForegroundColor Red
    exit 1
}

Write-Host ""
Write-Host "==================================" -ForegroundColor Cyan
Write-Host "  Build Completed Successfully!" -ForegroundColor Green
Write-Host "==================================" -ForegroundColor Cyan
Write-Host ""
Write-Host "Output:" -ForegroundColor White
Write-Host "  - Publish folder: bin\Release\net8.0-windows\win-x64\publish\" -ForegroundColor Gray
Write-Host "  - Desktop copy: $destPath" -ForegroundColor Gray
Write-Host ""
Write-Host "You can now run ImageLab.exe from your Desktop!" -ForegroundColor Yellow
Write-Host ""
