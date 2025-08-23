@{
    AliasesToExport        = @()
    Author                 = 'Przemyslaw Klys'
    CmdletsToExport        = @('Get-IISParsedLog')
    CompanyName            = 'Evotec'
    CompatiblePSEditions   = @('Desktop', 'Core')
    Copyright              = '(c) 2011 - 2025 Przemyslaw Klys @ Evotec. All rights reserved.'
    Description            = 'Module for parsing IIS logs'
    DotNetFrameworkVersion = '4.7.2'
    FunctionsToExport      = @()
    GUID                   = '798a1c8a-b4fd-4849-81d2-6138e39eb88b'
    ModuleVersion          = '1.0.0'
    PowerShellVersion      = '5.1'
    PrivateData            = @{
        PSData = @{
            ProjectUri = 'https://github.com/EvotecIT/IISParser'
            Tags       = @('Windows', 'IIS', 'parser', 'LogParser')
        }
    }
    RootModule             = 'IISParser.psm1'
}