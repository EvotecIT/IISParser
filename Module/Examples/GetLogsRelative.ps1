Import-Module $PSScriptRoot\..\IISParser.psd1 -Force

$logDir = Join-Path $PSScriptRoot '..\..\IISParser.Tests\TestData'
Push-Location $logDir
Get-IISParsedLog -FilePath '.\sample.log' -First 1 | Format-Table
Pop-Location
