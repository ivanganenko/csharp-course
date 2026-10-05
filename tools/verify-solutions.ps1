<#
  Проверка эталонных решений (для автора курса, не для студента).
  Для каждого модуля: копирует starter во временную папку .verify/, накладывает файлы
  из .teacher/solutions/NN/ (та же структура папок, что в starter/) и запускает dotnet test.
  Почему так: тесты в starter должны падать (там NotImplementedException), а с эталоном — проходить.
  Это гарантирует, что задачи решаемы и тесты корректны.

  Запуск из корня курса:  pwsh tools/verify-solutions.ps1        (все модули)
                          pwsh tools/verify-solutions.ps1 -Module 05
#>
param([string]$Module = "")

$ErrorActionPreference = "Stop"
$root = Split-Path -Parent $PSScriptRoot
$verify = Join-Path $root ".verify"
$failed = @()

Get-ChildItem (Join-Path $root "modules") -Directory | Sort-Object Name | ForEach-Object {
    $nn = $_.Name.Substring(0, 2)
    if ($Module -and $nn -ne $Module) { return }
    $starter = Join-Path $_.FullName "starter"
    $solution = Join-Path $root ".teacher/solutions/$nn"
    if (-not (Test-Path $starter) -or -not (Test-Path $solution)) { return }

    Write-Host "=== Module $nn ===" -ForegroundColor Cyan
    $target = Join-Path $verify $nn
    if (Test-Path $target) { Remove-Item $target -Recurse -Force }
    New-Item -ItemType Directory -Path $target | Out-Null
    Copy-Item "$starter/*" $target -Recurse
    # Directory.Build.props нужен и во временной копии
    Copy-Item (Join-Path $root "Directory.Build.props") $verify -Force

    # .teacher/solutions/NN/ повторяет структуру starter/ — накладываем поверх
    Copy-Item "$solution/*" $target -Recurse -Force

    Push-Location $target
    dotnet test --nologo
    if ($LASTEXITCODE -ne 0) { $failed += $nn }
    Pop-Location
}

if ($failed.Count -gt 0) {
    Write-Host "FAILED modules: $($failed -join ', ')" -ForegroundColor Red
    exit 1
}
Write-Host "All solutions pass." -ForegroundColor Green
