using System.Diagnostics.CodeAnalysis;
using System.Runtime.InteropServices;
using ClangSharp.Interop;

// ReSharper disable CommentTypo
// ReSharper disable StringLiteralTypo

namespace DUMPSYM.Tests;

[TestClass]
public sealed class UnitTestClangParsing2 : UnitTestBase
{
    [TestMethod]
    public unsafe void TestParseDirectory()
    {
        const string path = @"C:\Temp\PSX SDKs\extracted\PsyQ_Runtime_Library_4.7\INCLUDE";

        var files = Directory.GetFiles(path, "*.H", SearchOption.AllDirectories);

        var outputs = new List<Output>(files.Length);

        using var index = CXIndex.Create();

        var args = GetDefaultArguments(path);

        foreach (var file in files)
        {
            var unit = CXTranslationUnit.Parse(index, file, args.ToArray(), [], CXTranslationUnit_Flags.CXTranslationUnit_None);

            var output = new Output(file, unit);

            outputs.Add(output);

            var handle = GCHandle.Alloc(output);

            unit.Cursor.VisitChildren(Visit, new CXClientData(GCHandle.ToIntPtr(handle)));

            handle.Free();
        }

        foreach (var output in outputs)
        {
            WriteLine($"{output.File}: {output.Unit.DiagnosticSet.Count}");

            output.Dispose();
        }

        Assert.AreEqual(files.Length, outputs.Count);

        return;

        static CXChildVisitResult Visit(CXCursor cursor, CXCursor parent, void* data)
        {
            var output = (Output)GCHandle.FromIntPtr(new IntPtr(data)).Target!;

            output.Cursors.Add(cursor);

            return cursor.Kind switch
            {
                CXCursorKind.CXCursor_LinkageSpec => CXChildVisitResult.CXChildVisit_Recurse,
                _                                 => CXChildVisitResult.CXChildVisit_Continue,
            };
        }
    }

    public static List<string> GetDefaultArguments(params string[] includes)
    {
        var args = new List<string>
        {
            "-D", "_SIZE_T",                 // typedef redefinition with different types ('unsigned int' vs 'unsigned long long'): Line 69, Column 22 in SYS/TYPES.H
            "-D", "_WCHAR_T",                // 'long wchar_t' is invalid: Line 19, Column 18 in STDDEF.H
            "-Wno-nonportable-include-path", // KERNEL.H
        };

        foreach (var path in includes)
        {
            args.AddRange(["-I", path]);
        }

        return args;
    }
}

public sealed class Output : IDisposable
{
    public Output(string file, CXTranslationUnit unit)
    {
        File = file;
        Unit = unit;
    }

    public string File { get; }

    public CXTranslationUnit Unit { get; }

    public SortedSet<CXCursor> Cursors { get; } = new(CXCursorComparer.Instance);

    public void Dispose()
    {
        Unit.Dispose();
    }

    public override string ToString()
    {
        return $"{nameof(File)}: {File}";
    }

    [SuppressMessage("ReSharper", "InconsistentNaming")]
    private sealed class CXCursorComparer : Comparer<CXCursor>
    {
        public static CXCursorComparer Instance { get; } = new();

        public override int Compare(CXCursor x, CXCursor y)
        {
            x.Location.GetSpellingLocation(out _, out _, out _, out var xOffset);
            y.Location.GetSpellingLocation(out _, out _, out _, out var yOffset);

            return xOffset.CompareTo(yOffset);
        }
    }
}