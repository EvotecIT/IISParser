Import-Module $PSScriptRoot\..\IISParser.psd1 -Force

# Stream a subset of the log without loading the entire file into memory
Get-IISParsedLog -FilePath "C:\Support\GitHub\IISParser\Ignore\u_ex220507.log" -Skip 10 -First 50 -Last 5 | Format-Table
