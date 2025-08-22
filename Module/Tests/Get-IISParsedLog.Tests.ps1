$assemblyPath = (Resolve-Path "$PSScriptRoot/../../IISParser.PowerShell/bin/Debug/net8.0/IISParser.PowerShell.dll").Path
Import-Module $assemblyPath
$logPath = (Resolve-Path "$PSScriptRoot/../../IISParser.Tests/TestData/sample.log").Path
Describe 'Get-IISParsedLog' {
    It 'parses log file' {
        $result = Get-IISParsedLog -FilePath $logPath
        $result.csUriStem | Should -Be '/index.html'
        $result.scStatus | Should -Be 200
        $result.'X-Forwarded-For' | Should -Be '192.168.0.1'
        $result.Fields['X-Forwarded-For'] | Should -Be '192.168.0.1'
    }
}
