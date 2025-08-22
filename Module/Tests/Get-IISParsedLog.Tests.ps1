Describe 'Get-IISParsedLog' {
    It 'parses log file' {
        $logPath = (Resolve-Path "$PSScriptRoot/../../IISParser.Tests/TestData/sample.log").Path
        $result = Get-IISParsedLog -FilePath $logPath
        $result.csUriStem | Should -Be '/index.html'
        $result.scStatus | Should -Be 200
        $result.'X-Forwarded-For' | Should -Be '192.168.0.1'
        $result.Fields['X-Forwarded-For'] | Should -Be '192.168.0.1'
    }
}
