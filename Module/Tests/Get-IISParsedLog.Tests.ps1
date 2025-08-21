$assemblyPath = Join-Path $PSScriptRoot '..' '..' 'src' 'IISParser.PowerShell' 'bin' 'Debug' 'net8.0' 'IISParser.PowerShell.dll'
Import-Module $assemblyPath
$logPath = Join-Path $PSScriptRoot '..' '..' 'tests' 'IISParser.Tests' 'TestData' 'sample.log'
Describe 'Get-IISParsedLog' {
    It 'parses log file' {
        $result = Get-IISParsedLog -FilePath $logPath
        $result.csUriStem | Should -Be '/index.html'
        $result.scStatus | Should -Be 200
        $result.'X-Forwarded-For' | Should -Be '192.168.0.1'
        $result.Fields['X-Forwarded-For'] | Should -Be '192.168.0.1'
    }
}
