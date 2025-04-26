using System.Diagnostics.CodeAnalysis;
using System.Runtime.InteropServices;
using ClangSharp.Interop;
using DUMPSYM.Tests.WorkInProgress;

namespace DUMPSYM.Tests;

[TestClass]
[SuppressMessage("ReSharper", "StringLiteralTypo")]
[SuppressMessage("ReSharper", "CommentTypo")]
[SuppressMessage("ReSharper", "GrammarMistakeInComment")]
public sealed partial class UnitTestClangParsing
{
    private void Test(Header header)
    {
        const string directory = @"C:\Temp\PSX SDKs\extracted\PsyQ_Runtime_Library_4.7\INCLUDE";

        using var unit = Parse(directory, header, out var cursors);

        var diagnostics = unit.DiagnosticSet;

        foreach (var diagnostic in diagnostics)
        {
            WriteLine($"{diagnostic.Severity}:\n\t{diagnostic}:\n\t\t{diagnostic.Location}");
        }

        Assert.IsFalse(diagnostics.Any(s => s.Severity is CXDiagnosticSeverity.CXDiagnostic_Error or CXDiagnosticSeverity.CXDiagnostic_Fatal));

        foreach (var cursor in cursors)
        {
            cursor.Location.GetSpellingLocation(out var file, out var line, out var column, out var offset);

            WriteLine($"{cursor}, {cursor.Kind}, {file}");
        }
    }

    private static unsafe CXTranslationUnit Parse(string directory, Header header, out List<CXCursor> result)
    {
        var args = new List<string>
        {
            "-I", directory,
            "-D", "_SIZE_T",                // typedef redefinition with different types ('unsigned int' vs 'unsigned long long'): Line 69, Column 22 in SYS/TYPES.H
            "-D", "_WCHAR_T",               // 'long wchar_t' is invalid: Line 19, Column 18 in STDDEF.H
            "-Wno-nonportable-include-path" // KERNEL.H
        };

        if (!Sorting.TryGetTopologicalSort(header, s => s, out var dependencies))
        {
            throw new InvalidOperationException("Header topological sort failed.");
        }

        foreach (var dependency in dependencies)
        {
            args.AddRange(["-include", dependency.Path]);
        }

        result = [];

        var handle = GCHandle.Alloc(result);

        try
        {
            using var index = CXIndex.Create();

            var name = Path.Combine(directory, header.Path);

            var unit = CXTranslationUnit.Parse(index, name, CollectionsMarshal.AsSpan(args), [], CXTranslationUnit_Flags.CXTranslationUnit_None);

            unit.Cursor.VisitChildren(Visitor, new CXClientData(GCHandle.ToIntPtr(handle)));

            return unit;
        }
        finally
        {
            handle.Free();
        }
    }

    private static unsafe CXChildVisitResult Visitor(CXCursor cursor, CXCursor parent, void* data)
    {
        var handle = GCHandle.FromIntPtr(new IntPtr(data));

        var list = (List<CXCursor>)handle.Target!;

        switch (cursor.Kind)
        {
            case CXCursorKind.CXCursor_StructDecl or CXCursorKind.CXCursor_TypedefDecl:
                list.Add(cursor);
                break;
        }

        return CXChildVisitResult.CXChildVisit_Continue;
    }

    [TestMethod]
    public void TestManyHeaders()
    {
        var headers = new[]
        {
            Headers.FS,
            Headers.KERNEL,
            Headers.LIBCD,
            Headers.LIBDS,
            Headers.LIBGPU,
            Headers.LIBGS,
            Headers.LIBGTE,
            Headers.LIBHMD,
            Headers.LIBMCRD,
            Headers.LIBPRESS,
            Headers.LIBSND,
            Headers.LIBSPU,
            Headers.MCGUI,
            Headers.SETJMP,
            Headers.STDARG,
            Headers.STDDEF,
            Headers.TYPES
        };

        var sort = Sorting.TryGetTopologicalSort(headers, s => s, out var result);

        Assert.IsTrue(sort);

        result.ForEach(WriteLine);
    }
}