# IISParser - PowerShell Module

<p align="center">
  <a href="https://www.powershellgallery.com/packages/IISParser"><img src="https://img.shields.io/powershellgallery/v/IISParser.svg?style=flat-square"></a>
  <a href="https://www.powershellgallery.com/packages/IISParser"><img src="https://img.shields.io/powershellgallery/vpre/IISParser.svg?label=powershell%20gallery%20preview&colorB=yellow&style=flat-square"></a>
  <a href="https://github.com/EvotecIT/IISParser"><img src="https://img.shields.io/github/license/EvotecIT/IISParser.svg?style=flat-square"></a>
</p>

<p align="center">
  <a href="https://www.powershellgallery.com/packages/IISParser"><img src="https://img.shields.io/powershellgallery/p/IISParser.svg?style=flat-square"></a>
  <a href="https://github.com/EvotecIT/IISParser"><img src="https://img.shields.io/github/languages/top/evotecit/IISParser.svg?style=flat-square"></a>
  <a href="https://github.com/EvotecIT/IISParser"><img src="https://img.shields.io/github/languages/code-size/evotecit/IISParser.svg?style=flat-square"></a>
  <a href="https://www.powershellgallery.com/packages/IISParser"><img src="https://img.shields.io/powershellgallery/dt/IISParser.svg?style=flat-square"></a>
</p>

<p align="center">
  <a href="https://twitter.com/PrzemyslawKlys"><img src="https://img.shields.io/twitter/follow/PrzemyslawKlys.svg?label=Twitter%20%40PrzemyslawKlys&style=flat-square&logo=twitter"></a>
  <a href="https://evotec.xyz/hub"><img src="https://img.shields.io/badge/Blog-evotec.xyz-2A6496.svg?style=flat-square"></a>
  <a href="https://www.linkedin.com/in/pklys"><img src="https://img.shields.io/badge/LinkedIn-pklys-0077B5.svg?logo=LinkedIn&style=flat-square"></a>
</p>

**IISParser** is a **PowerShell** module to read IIS logs. It's very fast and easy to use. This module is based on [IISLogParser](https://github.com/Kabindas/IISLogParser) library that does all the heavy lifting.

## To install

```powershell
Install-Module -Name IISParser -AllowClobber -Force
```

Force and AllowClobber aren't necessary, but they do skip errors in case some appear.

## And to update

```powershell
Update-Module -Name IISParser
```

That's it. Whenever there's a new version, you run the command, and you can enjoy it. Remember that you may need to close, reopen PowerShell session if you have already used module before updating it.

**The essential thing** is if something works for you on production, keep using it till you test the new version on a test computer. I do changes that may not be big, but big enough that auto-update may break your code. For example, a small rename to a parameter, and your code stops working! Be responsible!


### Usage

Using IISParser module is very simple. Just import the module, and read the file as required.

```powershell
Import-Module IISParser

Get-IISParsedLog -FilePath "C:\Support\GitHub\IISParser\Ignore\u_ex220507.log" | Select-Object -First 5 | Format-Table
Get-IISParsedLog -FilePath "C:\Support\GitHub\IISParser\Ignore\u_ex220507.log" | Select-Object -Last 5 | Format-Table
Get-IISParsedLog -FilePath "C:\Support\GitHub\IISParser\Ignore\u_ex220507.log" -First 5 -Last 5 -Skip 1 | Format-Table
````