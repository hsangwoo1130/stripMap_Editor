# StripMap Editor - Release 빌드 및 패키지 생성 스크립트
# 사용법: PowerShell에서 실행 (installer 폴더 또는 프로젝트 루트에서)

$version      = "1.1.0"
$msbuild      = "C:\Program Files\Microsoft Visual Studio\2022\Community\MSBuild\Current\Bin\MSBuild.exe"
$solution     = "$PSScriptRoot\..\stripMap_Editor.sln"
$releaseDir   = "$PSScriptRoot\..\bin\Release"
$outputDir    = "$PSScriptRoot"
$zipName      = "StripMapEditor_v${version}_Package.zip"
$iscc         = "C:\Program Files (x86)\Inno Setup 6\ISCC.exe"
$issScript    = "$PSScriptRoot\stripMap_Editor_setup.iss"

Write-Host "=== StripMap Editor v$version Release Build ===" -ForegroundColor Cyan

# 1. Release 빌드
Write-Host "`n[1/3] MSBuild Release 빌드 중..." -ForegroundColor Yellow
& $msbuild $solution /p:Configuration=Release /t:Build /v:minimal
if ($LASTEXITCODE -ne 0) { Write-Host "빌드 실패" -ForegroundColor Red; exit 1 }
Write-Host "빌드 완료" -ForegroundColor Green

# 2. ZIP 패키지 생성 (pdb/xml 제외)
Write-Host "`n[2/3] ZIP 패키지 생성 중..." -ForegroundColor Yellow
$zipPath = Join-Path $outputDir $zipName
if (Test-Path $zipPath) { Remove-Item $zipPath }

$include = @("*.exe", "*.config", "*.dll", "*.netmodule", "config.ini")
$files = $include | ForEach-Object { Get-Item "$releaseDir\$_" -ErrorAction SilentlyContinue } | Where-Object { $_ }
Compress-Archive -Path $files.FullName -DestinationPath $zipPath
Write-Host "ZIP 생성 완료: $zipName" -ForegroundColor Green

# 3. 설치 파일 생성 (Inno Setup)
Write-Host "`n[3/3] 설치 파일 생성 중..." -ForegroundColor Yellow
if (Test-Path $iscc) {
    & $iscc $issScript
    if ($LASTEXITCODE -eq 0) {
        Write-Host "설치 파일 생성 완료: StripMapEditor_v${version}_Setup.exe" -ForegroundColor Green
    } else {
        Write-Host "설치 파일 생성 실패" -ForegroundColor Red
    }
} else {
    Write-Host "Inno Setup이 설치되어 있지 않습니다. ZIP만 생성되었습니다." -ForegroundColor Yellow
    Write-Host "다운로드: https://jrsoftware.org/isdl.php" -ForegroundColor Gray
}

Write-Host "`n=== 완료 ===" -ForegroundColor Cyan
Write-Host "출력 경로: $outputDir" -ForegroundColor Gray
