using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;

namespace AlmostMaximize;

internal static class CustomPercentageStore
{
    private static readonly string DirectoryPath = Path.Combine(
        Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
        "AlmostMaximize");
    private static readonly string FilePath = Path.Combine(DirectoryPath, "custom-percentages.txt");

    public static IReadOnlyList<int> Load()
    {
        try
        {
            if (!File.Exists(FilePath))
            {
                return [];
            }

            var values = File.ReadAllLines(FilePath)
                .Select(static line => int.TryParse(line, out var value) ? value : 0);
            return values
                .Where(IsValid)
                .Distinct()
                .OrderByDescending(static value => value)
                .ToArray();
        }
        catch
        {
            return [];
        }
    }

    public static void Save(IEnumerable<int> values)
    {
        Directory.CreateDirectory(DirectoryPath);

        var normalized = values
            .Where(IsValid)
            .Distinct()
            .OrderByDescending(static value => value)
            .ToArray();

        File.WriteAllLines(FilePath, normalized.Select(static value => value.ToString(CultureInfo.InvariantCulture)));
    }

    public static bool IsValid(int value) =>
        value >= WindowResizer.MinPercentage && value <= WindowResizer.MaxPercentage;
}
