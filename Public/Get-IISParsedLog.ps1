function Get-IISParsedLog {
    <#
    .SYNOPSIS
    Parses IIS log and provides it as native PowerShell object

    .DESCRIPTION
    Parses IIS log and provides it as native PowerShell object

    .PARAMETER FilePath
    FilePath/LogPath to file to read

    .PARAMETER First
    Show only X amount of lines from the beginning of the file

    .PARAMETER Last
    Show only X amount of lines from the end of the file

    .PARAMETER Skip
    Skip X amount of lines from the beginning of the file

    .PARAMETER SkipLast
    Skip X amount of lines from the end of the file

    .EXAMPLE
    Get-IISParsedLog -FilePath "C:\Support\GitHub\IISParser\Ignore\u_ex220507.log" -First 5 -Last 5 -Skip 1 | Format-Table

    .EXAMPLE
    Get-IISLog -FilePath "C:\Support\GitHub\IISParser\Ignore\u_ex220507.log" | Select-Object -First 5 | Format-Table

    .EXAMPLE
    Get-IISLog -FilePath "C:\Support\GitHub\IISParser\Ignore\u_ex220507.log" | Select-Object -Last 5 | Format-Table

    .NOTES
    General notes
    #>
    [cmdletBinding()]
    param(
        [parameter(Mandatory)][alias('LogPath')][string] $FilePath,
        [parameter(ParameterSetName = 'FirstLastSkip')]
        [int] $First,
        [parameter(ParameterSetName = 'FirstLastSkip')]
        [int] $Last,
        [parameter(ParameterSetName = 'FirstLastSkip')]
        [int] $Skip,
        [parameter(ParameterSetName = 'SkipLast')]
        [int] $SkipLast
    )
    Begin {
        if (Test-Path -LiteralPath $FilePath -ErrorAction SilentlyContinue) {
            try {
                $LogParsing = [IISLogParser.ParserEngine]::new($FilePath)
            } catch {
                if ($PSBoundParameters.ErrorAction -eq 'Stop') {
                    throw
                } else {
                    $ErrorMessage = $_.Exception.Message -replace "`n", " " -replace "`r", " "
                    Write-Warning -Message "Get-IISParsedLog - Couldn't initialize parser. Error: $ErrorMessage"
                    return
                }
            }
        }
    }
    Process {
        if ($LogParsing) {
            try {
                if ($First -or $Last -or $Skip) {
                    $SplatObject = @{
                        First = $First
                        Last  = $Last
                        Skip  = $Skip
                    }
                    $LogParsing.ParseLog() | Select-Object @SplatObject
                } elseif ($SkipLast) {
                    $LogParsing.ParseLog() | Select-Object -SkipLast $SkipLast
                } else {
                    $LogParsing.ParseLog()
                }
            } catch {
                if ($PSBoundParameters.ErrorAction -eq 'Stop') {
                    throw
                } else {
                    $ErrorMessage = $_.Exception.Message -replace "`n", " " -replace "`r", " "
                    Write-Warning -Message "Get-IISParsedLog - Couldn't parse log. Error: $ErrorMessage"
                    return
                }
            }
        }
    }
    End {
        if ($LogParsing) {
            try {
                $LogParsing.Dispose()
            } catch {
                if ($PSBoundParameters.ErrorAction -eq 'Stop') {
                    throw
                } else {
                    $ErrorMessage = $_.Exception.Message -replace "`n", " " -replace "`r", " "
                    Write-Warning -Message "Get-IISParsedLog - Couldn't dispose log. Error: $ErrorMessage"
                    return
                }
            }
        }
    }
}