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

Output of the above command will look like this:

```
DateTimeEvent       sSitename sComputername sIp           csMethod     csUriStem                                                                       csUriQuery                                                            sPort csUsername          cIp
-------------       --------- ------------- ---           --------     ---------                                                                       ----------                                                            ----- ----------          ---
07.05.2022 00:00:20                         127.0.0.1    GET          /api/v1.0/users/HealthMailboxbfeedfb17e384aa0bb8effaf5d7849a8@euvic.pl/Messages $top=1&request_id=26cca5c9-37b6-4d0b-8ae1-31e5cb7ccc72                  444                     127.0.0.1
07.05.2022 00:00:22                         172.16.1.223 RPC_IN_DATA  /rpc/rpcproxy.dll                                                               Exch1.EVOTEC.XYZ:6001&RequestId=9e1ca7e6-278e-4b98-a9c0-be947b39a582    81                      172.16.1.223
07.05.2022 00:00:22                         172.16.1.223 RPC_OUT_DATA /rpc/rpcproxy.dll                                                               Exch1.EVOTEC.XYZ:6001&RequestId=3acb4929-df94-48c0-bbad-f5f2a39f0ac3    81                      172.16.1.223
07.05.2022 00:00:26                         172.16.1.223 POST         /mapi/emsmdb/                                                                   MailboxId=04f2c908-9690-4be9-93bc-43a16635d8ad@junisoftex.pl            444 Anonymous           172.16.1.223
07.05.2022 00:00:27                         172.16.1.223 POST         /autodiscover/autodiscover.xml                                                  &reqId=e4639aef-215b-420f-8100-1e2f8eca8bcb                             444 EVOTEC\masul        172.16.1.223
```

Of course there are a bit more properties available:

```powershell
Get-IISParsedLog -FilePath "C:\Support\GitHub\IISParser\Ignore\u_ex220507.log" -First 1 -Skip 1 | Format-List
```

Output of the above command will look like this:

```
DateTimeEvent : 07.05.2022 00:00:22
sSitename     :
sComputername :
sIp           : 172.16.1.223
csMethod      : RPC_IN_DATA
csUriStem     : /rpc/rpcproxy.dll
csUriQuery    : Exch1.EVOTEC.XYZ:6001&RequestId=9e1ca7e6-278e-4b98-a9c0-be947b39a582
sPort         : 81
csUsername    :
cIp           : 172.16.1.223
csVersion     :
csUserAgent   : MSRPC
csCookie      :
csReferer     :
csHost        :
scStatus      : 401
scSubstatus   : 1
scWin32Status : 2148074254
scBytes       :
csBytes       :
timeTaken     : 64
```