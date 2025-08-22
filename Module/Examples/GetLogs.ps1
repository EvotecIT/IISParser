Import-Module $PSScriptRoot\..\IISParser.psd1 -Force

Get-IISParsedLog -FilePath "C:\Support\GitHub\IISParser\Ignore\u_ex220507.log" | Select-Object -First 5 | Format-Table
Get-IISParsedLog -FilePath "C:\Support\GitHub\IISParser\Ignore\u_ex220507.log" | Select-Object -Last 5 | Format-Table
Get-IISParsedLog -FilePath "C:\Support\GitHub\IISParser\Ignore\u_ex220507.log" -First 5 -Last 5 -Skip 1 | Format-Table
Get-IISParsedLog -FilePath "C:\Support\GitHub\IISParser\Ignore\u_ex220507.log" -Expand | Select-Object -First 1 | Format-List
