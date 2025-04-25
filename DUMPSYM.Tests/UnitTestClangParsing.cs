using System.Diagnostics.CodeAnalysis;
using System.Runtime.InteropServices;
using ClangSharp.Interop;
using DUMPSYM.Tests.WorkInProgress;

namespace DUMPSYM.Tests;

[TestClass]
[SuppressMessage("ReSharper", "StringLiteralTypo")]
[SuppressMessage("ReSharper", "CommentTypo")]
[SuppressMessage("ReSharper", "GrammarMistakeInComment")]
public sealed partial class UnitTestClangParsing : UnitTestBase
{
    private static string SdkDir { get; } = @"C:\Temp\PSX SDKs\extracted\PsyQ_Runtime_Library_4.7";

    private static string SdkDirInclude { get; } = Path.Combine(SdkDir, "INCLUDE");

    private unsafe void Test(Header header)
        // KERNEL.H // BUG these have no typedefs
    {
        using var index = CXIndex.Create();

        var args = new List<string>
        {
            "-I", SdkDirInclude,
            "-D", "_SIZE_T",                // typedef redefinition with different types ('unsigned int' vs 'unsigned long long'): Line 69, Column 22 in SYS/TYPES.H
            "-D", "_WCHAR_T",               // 'long wchar_t' is invalid: Line 19, Column 18 in STDDEF.H
            "-Wno-nonportable-include-path" // KERNEL.H
        };

        var sort = Sorting.TryGetTopologicalSort(header, s => s, out var result);

        Assert.IsTrue(sort);

        foreach (var dependency in result)
        {
            WriteLineVar(dependency);

            Assert.AreNotEqual(header.Path, dependency.Path);

            args.AddRange(["-include", dependency.Path]);
        }

        var cursors = new List<CXCursor>();

        var sourceFileName = Path.Combine(SdkDirInclude, header.Path);

        using (var unit = CXTranslationUnit.Parse(index, sourceFileName, CollectionsMarshal.AsSpan(args), [], CXTranslationUnit_Flags.CXTranslationUnit_None))
        {
            var diagnostics = unit.DiagnosticSet;

            foreach (var diagnostic in diagnostics)
            {
                WriteLine($"{diagnostic.Severity}:\n\t{diagnostic}:\n\t\t{diagnostic.Location}");
            }

            Assert.IsFalse(diagnostics.Any(s => s.Severity is CXDiagnosticSeverity.CXDiagnostic_Error or CXDiagnosticSeverity.CXDiagnostic_Fatal));

            var handle = GCHandle.Alloc(cursors);

            unit.Cursor.VisitChildren(Visitor, new CXClientData(GCHandle.ToIntPtr(handle)));

            handle.Free();
        }

        Assert.AreNotEqual(0, cursors.Count, "No symbols found.");
    }

    private unsafe CXChildVisitResult Visitor(CXCursor cursor, CXCursor parent, void* data)
    {
        var handle = GCHandle.FromIntPtr(new IntPtr(data));

        var list = (List<CXCursor>)handle.Target!;

        cursor.Location.GetSpellingLocation(out var file, out var line, out var column, out _);

        var info = $"File: {file.ToString()[(SdkDir.Length + 1)..]}, " +
                   $"Line: {line}, " +
                   $"Column: {column}, " +
                   $"Kind: {cursor.Kind}, " +
                   $"Name: {cursor.DisplayName}";

        switch (cursor.Kind)
        {
            case CXCursorKind.CXCursor_StructDecl:
                list.Add(cursor);
                WriteLine($"{info}");
                break;
            case CXCursorKind.CXCursor_TypedefDecl:
                WriteLine($"{info} -> {cursor.TypedefDeclUnderlyingType}");
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