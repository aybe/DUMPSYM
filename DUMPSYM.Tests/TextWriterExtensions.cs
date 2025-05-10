using System.CodeDom.Compiler;
using System.Reflection;
using System.Runtime.CompilerServices;

namespace DUMPSYM.Tests;

public static class TextWriterExtensions
{
    private static FieldInfo TabString { get; } =
        typeof(IndentedTextWriter)
            .GetField("_tabString", BindingFlags.Instance | BindingFlags.NonPublic)!;

    private static ConditionalWeakTable<IndentedTextWriter, string> Table { get; } = new();

    public static void WriteLine2(this IndentedTextWriter writer, object? x, object? y, int padding = 60)
    {
        ArgumentNullException.ThrowIfNull(writer);

        ArgumentOutOfRangeException.ThrowIfNegative(padding);

        var chars = Table.GetValue(writer, s => (string)TabString.GetValue(s)!);

        var width = Math.Max(0, padding - writer.Indent * chars.Length);

        var value = $"{x?.ToString()?.PadRight(width)}{y}";

        writer.WriteLine(value);
    }
}