@{
    AliasesToExport        = @()
    Author                 = 'Przemyslaw Klys'
    CmdletsToExport        = @()
    CompanyName            = 'Evotec'
    CompatiblePSEditions   = @('Desktop', 'Core')
    Copyright              = '(c) 2011 - 2022 Przemyslaw Klys @ Evotec. All rights reserved.'
    Description            = 'Module for parsing IIS logs'
    DotNetFrameworkVersion = '4.7.2'
    FunctionsToExport      = 'Get-IISParsedLog'
    GUID                   = '798a1c8a-b4fd-4849-81d2-6138e39eb88b'
    ModuleVersion          = '0.0.1'
    PowerShellVersion      = '5.1'
    PrivateData            = @{
        PSData = @{
            Tags       = @('Windows', 'IIS', 'parser')
            ProjectUri = 'https://github.com/EvotecIT/IISParser'
        }
    }
    RootModule             = 'IISParser.psm1'
}