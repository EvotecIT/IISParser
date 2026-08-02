---
external help file: IISParser-help.xml
Module Name: IISParser
online version: https://github.com/EvotecIT/IISParser
schema: 2.0.0
---
# Get-IISParsedLog
## SYNOPSIS
Parses entries from an IIS log file.

## SYNTAX
### Default (Default)
```powershell
Get-IISParsedLog -FilePath <string> [-Expand] [-Legacy] [-MaxRecords <int>] [<CommonParameters>]
```

### FirstLastSkip
```powershell
Get-IISParsedLog -FilePath <string> [-First <int>] [-Last <int>] [-Skip <int>] [-Expand] [-Legacy] [-MaxRecords <int>] [<CommonParameters>]
```

### SkipLast
```powershell
Get-IISParsedLog -FilePath <string> [-SkipLast <int>] [-Expand] [-Legacy] [-MaxRecords <int>] [<CommonParameters>]
```

## DESCRIPTION
Reads the specified log and converts each record into a PowerShell object for further processing.

Entries are streamed lazily to minimize memory usage.

Use filtering parameters to limit the number of events returned.

## EXAMPLES

### EXAMPLE 1
```powershell
PS> Get-IISParsedLog -FilePath "C:\\Logs\\u_ex230101.log"
```

Outputs all entries from the specified log.

### EXAMPLE 2
```powershell
PS> Get-IISParsedLog -FilePath "C:\\Logs\\u_ex230101.log" -Skip 10 -First 5
```

Skips the first ten lines and returns the next five.

### EXAMPLE 3
```powershell
PS> Get-IISParsedLog -FilePath "C:\\Logs\\u_ex230101.log" -Expand
```

Field names such as X-Forwarded-For become X_Forwarded_For.

### EXAMPLE 4
```powershell
PS> Get-IISParsedLog -FilePath "C:\\Logs\\u_ex230101.log" -Legacy
```

Outputs entries using the original property names.

### EXAMPLE 5
```powershell
PS> Get-IISParsedLog -FilePath "C:\\Logs\\u_ex230101.log" -MaxRecords 50000
```

Reads only the first 50,000 entries before stopping.

## PARAMETERS

### -Expand
Expands fields into top-level properties.
Field names are transformed into PowerShell-friendly identifiers by
replacing - with _ and removing parentheses.

```yaml
Type: SwitchParameter
Parameter Sets: Default, FirstLastSkip, SkipLast
Aliases: None
Possible values:

Required: False
Position: named
Default value: None
Accept pipeline input: False
Accept wildcard characters: False
```

### -FilePath
Path to the IIS log file.

```yaml
Type: String
Parameter Sets: Default, FirstLastSkip, SkipLast
Aliases: LogPath
Possible values:

Required: True
Position: named
Default value: None
Accept pipeline input: False
Accept wildcard characters: False
```

### -First
Selects the first number of log entries to return.

```yaml
Type: Nullable`1
Parameter Sets: FirstLastSkip
Aliases: None
Possible values:

Required: False
Position: named
Default value: None
Accept pipeline input: False
Accept wildcard characters: False
```

### -Last
Returns only the last number of log entries.

```yaml
Type: Nullable`1
Parameter Sets: FirstLastSkip
Aliases: None
Possible values:

Required: False
Position: named
Default value: None
Accept pipeline input: False
Accept wildcard characters: False
```

### -Legacy
Outputs objects with legacy property names.

```yaml
Type: SwitchParameter
Parameter Sets: Default, FirstLastSkip, SkipLast
Aliases: None
Possible values:

Required: False
Position: named
Default value: None
Accept pipeline input: False
Accept wildcard characters: False
```

### -MaxRecords
Maximum number of records to read from the log file.
The default (null) reads the entire file.

```yaml
Type: Nullable`1
Parameter Sets: Default, FirstLastSkip, SkipLast
Aliases: None
Possible values:

Required: False
Position: named
Default value: None
Accept pipeline input: False
Accept wildcard characters: False
```

### -Skip
Skips a specified number of entries from the start.

```yaml
Type: Nullable`1
Parameter Sets: FirstLastSkip
Aliases: None
Possible values:

Required: False
Position: named
Default value: None
Accept pipeline input: False
Accept wildcard characters: False
```

### -SkipLast
Omits a specified number of entries from the end.

```yaml
Type: Nullable`1
Parameter Sets: SkipLast
Aliases: None
Possible values:

Required: False
Position: named
Default value: None
Accept pipeline input: False
Accept wildcard characters: False
```

### CommonParameters
This cmdlet supports the common parameters: -Debug, -ErrorAction, -ErrorVariable, -InformationAction, -InformationVariable, -OutVariable, -OutBuffer, -PipelineVariable, -Verbose, -WarningAction, and -WarningVariable. For more information, see [about_CommonParameters](http://go.microsoft.com/fwlink/?LinkID=113216).

## INPUTS

- `None`

## OUTPUTS

- `None`

## RELATED LINKS

- [https://learn.microsoft.com/iis/configuration/system.webserver/httplogging](https://learn.microsoft.com/iis/configuration/system.webserver/httplogging)
- [https://github.com/EvotecIT/IISParser](https://github.com/EvotecIT/IISParser)
