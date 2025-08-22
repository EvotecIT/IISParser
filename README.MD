# IISParser - Modern IIS Log Parser for .NET and PowerShell

IISParser is available as a PowerShell module from the PowerShell Gallery.
It is a **native C# rewrite with C# cmdlets**, completely dependency-free.
This project is inspired by the discontinued [IISLogParser](https://github.com/Kabindas/IISLogParser) by Kabindas.

📦 PowerShell Module

[![powershell gallery version](https://img.shields.io/powershellgallery/v/IISParser.svg)](https://www.powershellgallery.com/packages/IISParser)
[![powershell gallery preview](https://img.shields.io/powershellgallery/vpre/IISParser.svg?label=powershell%20gallery%20preview&colorB=yellow)](https://www.powershellgallery.com/packages/IISParser)
[![powershell gallery platforms](https://img.shields.io/powershellgallery/p/IISParser.svg)](https://www.powershellgallery.com/packages/IISParser)
[![powershell gallery downloads](https://img.shields.io/powershellgallery/dt/IISParser.svg)](https://www.powershellgallery.com/packages/IISParser)

🛠️ Project Information

[![top language](https://img.shields.io/github/languages/top/evotecit/IISParser.svg)](https://github.com/EvotecIT/IISParser)
[![license](https://img.shields.io/github/license/EvotecIT/IISParser.svg)](https://github.com/EvotecIT/IISParser)
[![build status](https://github.com/EvotecIT/IISParser/actions/workflows/test-dotnet.yml/badge.svg)](https://github.com/EvotecIT/IISParser/actions)
[![build status](https://github.com/EvotecIT/IISParser/actions/workflows/test-powershell.yml/badge.svg)](https://github.com/EvotecIT/IISParser/actions)
[![codecov](https://codecov.io/gh/EvotecIT/IISParser/branch/main/graph/badge.svg)](https://codecov.io/gh/EvotecIT/IISParser)

👨‍💻 Author & Social

[![Twitter follow](https://img.shields.io/twitter/follow/PrzemyslawKlys.svg?label=Twitter%20%40PrzemyslawKlys&style=social)](https://twitter.com/PrzemyslawKlys)
[![Blog](https://img.shields.io/badge/Blog-evotec.xyz-2A6496.svg)](https://evotec.xyz/hub)
[![LinkedIn](https://img.shields.io/badge/LinkedIn-pklys-0077B5.svg?logo=LinkedIn)](https://www.linkedin.com/in/pklys)
[![Threads](https://img.shields.io/badge/Threads-@PrzemyslawKlys-000000.svg?logo=Threads&logoColor=White)](https://www.threads.net/@przemyslaw.klys)
[![Discord](https://img.shields.io/discord/508328927853281280?style=flat-square&label=discord%20chat)](https://evo.yt/discord)

---

## What it's all about

**IISParser** is a modern, fast IIS log parser for PowerShell and .NET:

- Fully implemented in **C#**
- Provides **native PowerShell cmdlets**
- **No external dependencies**
- Cross-platform (Windows, Linux, macOS with PowerShell 7+)

Originally, **IISLogParser** handled IIS log parsing. After it was discontinued, IISParser continues the concept as a self-contained modern implementation.

---

## Installation

```powershell
Install-Module -Name IISParser -Scope CurrentUser
````

---

## Updating

```powershell
Update-Module -Name IISParser
```

*Tip: Restart PowerShell if the module was already loaded.*

---

## Usage Examples

```powershell
Import-Module IISParser

# Get first and last 5 lines from IIS log
Get-IISParsedLog -FilePath "C:\Logs\u_ex220507.log" | Select-Object -First 5
Get-IISParsedLog -FilePath "C:\Logs\u_ex220507.log" | Select-Object -Last 5

# Skip and filter
Get-IISParsedLog -FilePath "C:\Logs\u_ex220507.log" -First 5 -Last 5 -Skip 1
```

Detailed output:

```powershell
Get-IISParsedLog -FilePath "C:\Logs\u_ex220507.log" -First 1 -Skip 1 | Format-List
```


## Credits

* Original inspiration: **IISLogParser** by Kabindas