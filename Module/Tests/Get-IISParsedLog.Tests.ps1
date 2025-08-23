Describe 'Get-IISParsedLog' {
    It 'returns native object by default' {
        $logPath = (Resolve-Path "$PSScriptRoot/../../IISParser.Tests/TestData/sample.log").Path
        $result = Get-IISParsedLog -FilePath $logPath
        $result.csUriStem | Should -Be '/index.html'
        $result.scStatus | Should -Be 200
        ($result.PSObject.Properties.Match('X-Forwarded-For').Count) | Should -Be 0
        ($result.PSObject.Properties.Match('xMy_Field').Count) | Should -Be 0
        $result.Fields['X-Forwarded-For'] | Should -Be '192.168.0.1'
        $result.Fields['x(My-Field)'] | Should -Be 'Value'
    }

    It 'expands fields when requested' {
        $logPath = (Resolve-Path "$PSScriptRoot/../../IISParser.Tests/TestData/sample.log").Path
        $result = Get-IISParsedLog -FilePath $logPath -Expand
        $result.X_Forwarded_For | Should -Be '192.168.0.1'
        $result.xMy_Field | Should -Be 'Value'
        ($result.PSObject.Properties.Match('Fields').Count) | Should -Be 0
    }

    It 'streams large log with Skip, First and Last' {
        $logPath = Join-Path $TestDrive 'large.log'
        $header = '#Fields: date time s-ip cs-method cs-uri-stem sc-status X-Forwarded-For'
        $entries = 0..999 | ForEach-Object { "2024-01-01 00:00:00 127.0.0.1 GET /index$_.html 200 192.168.0.1" }
        $header, $entries | Set-Content -Path $logPath

        $result = Get-IISParsedLog -FilePath $logPath -Skip 10 -First 50 -Last 5
        $result.Count | Should -Be 5
        $result[0].csUriStem | Should -Be '/index55.html'
        $result[-1].csUriStem | Should -Be '/index59.html'
    }

    It 'supports SkipLast on large log' {
        $logPath = Join-Path $TestDrive 'large2.log'
        $header = '#Fields: date time s-ip cs-method cs-uri-stem sc-status X-Forwarded-For'
        $entries = 0..999 | ForEach-Object { "2024-01-01 00:00:00 127.0.0.1 GET /index$_.html 200 192.168.0.1" }
        $header, $entries | Set-Content -Path $logPath

        $result = Get-IISParsedLog -FilePath $logPath -SkipLast 10
        $result.Count | Should -Be 990
        $result[-1].csUriStem | Should -Be '/index989.html'
    }

    It 'returns last records from large log' {
        $logPath = Join-Path $TestDrive 'large3.log'
        $header = '#Fields: date time s-ip cs-method cs-uri-stem sc-status X-Forwarded-For'
        $entries = 0..999 | ForEach-Object { "2024-01-01 00:00:00 127.0.0.1 GET /index$_.html 200 192.168.0.1" }
        $header, $entries | Set-Content -Path $logPath

        $result = Get-IISParsedLog -FilePath $logPath -Last 5
        $result.Count | Should -Be 5
        $result[0].csUriStem | Should -Be '/index995.html'
        $result[-1].csUriStem | Should -Be '/index999.html'
    }
}
