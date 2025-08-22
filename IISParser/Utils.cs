using System.Collections.Generic;
using System.IO;

namespace IISParser;

/// <summary>
/// Provides helper methods used by the parser.
/// </summary>
public static class Utils {
    /// <summary>
    /// Reads all lines from a file allowing for shared read access.
    /// </summary>
    /// <param name="file">The path to the file to read.</param>
    /// <returns>A list containing each line of the file.</returns>
    public static List<string> ReadAllLines(string file) {
        var lines = new List<string>();
        using var fileStream = File.Open(file, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
        using var reader = new StreamReader(fileStream);
        while (reader.Peek() > -1)
            lines.Add(reader.ReadLine() ?? string.Empty);
        return lines;
    }
}