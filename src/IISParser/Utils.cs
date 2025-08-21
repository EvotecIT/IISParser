using System.Collections.Generic;
using System.IO;

namespace IISParser;

public static class Utils
{
    public static List<string> ReadAllLines(string file)
    {
        var lines = new List<string>();
        using var fileStream = File.Open(file, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
        using var reader = new StreamReader(fileStream);
        while (reader.Peek() > -1)
            lines.Add(reader.ReadLine() ?? string.Empty);
        return lines;
    }
}
