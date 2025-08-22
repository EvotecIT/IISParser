Describe 'Get-IISParsedLog' {
    It 'returns native object by default' {
        $logPath = (Resolve-Path "$PSScriptRoot/../../IISParser.Tests/TestData/sample.log").Path
        $result = Get-IISParsedLog -FilePath $logPath
        $result.csUriStem | Should -Be '/index.html'
        $result.scStatus | Should -Be 200
        ($result.PSObject.Properties.Match('X-Forwarded-For').Count) | Should -Be 0
        $result.Fields['X-Forwarded-For'] | Should -Be '192.168.0.1'
    }

    It 'expands fields when requested' {
        $logPath = (Resolve-Path "$PSScriptRoot/../../IISParser.Tests/TestData/sample.log").Path
        $result = Get-IISParsedLog -FilePath $logPath -Expand
        $result.'X-Forwarded-For' | Should -Be '192.168.0.1'
    }
}
