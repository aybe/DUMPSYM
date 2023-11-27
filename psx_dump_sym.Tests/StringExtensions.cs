namespace psx_dump_sym.Tests;

public static class StringExtensions // TODO move to library
{
    public static IEnumerable<string> EnumerateLines(this string value)
    {
        using var reader = new StringReader(value);

        while (reader.ReadLine() is { } line)
        {
            yield return line;
        }
    }

    public static string[] ReadLines(this string value)
    {
        return value.EnumerateLines().ToArray();
    }

    public static IEnumerable<char> GetCommonPrefix(this string? value, string? other)
    {
        if (value == null || other == null)
        {
            yield break;
        }

        var min = Math.Min(value.Length, other.Length);

        for (var i = 0; i < min; i++)
        {
            if (value[i] != other[i])
            {
                yield break;
            }

            yield return value[i];
        }
    }
}