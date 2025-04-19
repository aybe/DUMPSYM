using System.Diagnostics.CodeAnalysis;
using System.Reflection;

namespace DUMPSYM.Tests;

public static class TestResultUtility
// works but too complex...
// also works but unpractical:
// - write file to results directory as usual, set it to read-only, it won't be deleted anymore
// - but it will be in LUT directory next to tons of empty directories
{
    public static string Example(TestContext ctx)
    {
        var directory = GetResultsDirectory(ctx, @"C:\Temp");

        var combine = Path.Combine(directory, "result.txt");

        return combine;
    }

    public static string GetResultsDirectory(TestContext ctx, string path)
    {
        var name = Assembly.GetExecutingAssembly().GetName().Name!;

        var directory = GetDeployDirectory(ctx.TestRunDirectory!);

        var combine = Path.Combine(path, name, "TestResults", directory);

        return combine;
    }

    private static string GetDeployDirectory(string path)
    {
        if (TryGetSubstring(path, @"\TestResults\", out var x))
        {
            return x;
        }

        if (TryGetSubstring(path, @"\v2\tst\", out var y))
        {
            return y;
        }

        throw new InvalidOperationException();
    }

    private static bool TryGetSubstring(string path, string pattern, [MaybeNullWhen(false)] out string result)
    {
        result = null;

        var index = path.IndexOf(pattern, StringComparison.Ordinal);

        if (index != -1)
        {
            result = path[(index + pattern.Length)..];
        }

        return result != null;
    }
}