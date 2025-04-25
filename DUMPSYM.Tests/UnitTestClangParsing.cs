using System.Diagnostics.CodeAnalysis;
using System.Runtime.InteropServices;
using ClangSharp.Interop;
using DUMPSYM.Tests.WorkInProgress;

namespace DUMPSYM.Tests;

[TestClass]
[SuppressMessage("ReSharper", "StringLiteralTypo")]
[SuppressMessage("ReSharper", "CommentTypo")]
[SuppressMessage("ReSharper", "GrammarMistakeInComment")]
public sealed class UnitTestClangParsing : UnitTestBase
{
    private static string SdkDir { get; } = @"C:\Temp\PSX SDKs\extracted\Programmer Tool - Runtime Library Version 4.6 (Japan)_DTL-S2360_redump\PSX";

    private static string SdkDirInclude { get; } = Path.Combine(SdkDir, "INCLUDE");

    [TestMethod]
    [DynamicData(nameof(TestSymbolSearchData))]
    public unsafe void TestSymbolSearch(Header header)
    {
        using var index = CXIndex.Create();

        var args = new List<string>
        {
            "-I", SdkDirInclude,
            "-D", "_SIZE_T" // SYS/TYPES.H:69 typedef redefinition with different types ('unsigned int' vs 'unsigned long long'):
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

    public static IEnumerable<object[]> TestSymbolSearchData()
    {
        // TODO // ABS.H
        // TODO // ASM.H
        // TODO // ASSERT.H
        // TODO // CONVERT.H
        // TODO // CTYPE.H
        // TODO // FS.H
        // TODO // GTEMAC.H
        // TODO // GTENOM.H
        // TODO // GTEREG_S.H
        // TODO // GTEREG.H
        // TODO // INLINE_A.H
        // TODO // INLINE_C.H
        // TODO // INLINE_O.H
        // TODO // INLINE_S.H
        yield return [Headers.KERNEL]; // KERNEL.H
        // TODO // LIBAPI.H
        yield return [Headers.LIBCD]; // LIBCD.H
        // TODO // LIBCOMB.H
        yield return [Headers.LIBDS]; // LIBDS.H
        // TODO // LIBETC.H
        yield return [Headers.LIBGPU]; // LIBGPU.H
        yield return [Headers.LIBGS];  // LIBGS.H
        yield return [Headers.LIBGTE]; // LIBGTE.H
        // TODO // LIBGUN.H
        yield return [Headers.LIBHMD]; // LIBHMD.H
        // TODO // LIBMATH.H
        yield return [Headers.LIBMCRD]; // LIBMCRD.H
        // TODO // LIBMCX.H
        // TODO // LIBPAD.H
        // TODO // LIBPRESS.H
        // TODO // LIBSIO.H
        // TODO // LIBSN.H
        yield return [Headers.LIBSND]; // LIBSND.H
        yield return [Headers.LIBSPU]; // LIBSPU.H
        // TODO // LIBTAP.H
        // TODO // LIMITS.H
        // TODO // MALLOC.H
        // TODO // MCGUI.H
        // TODO // MEMORY.H
        // TODO // QSORT.H
        // TODO // R3000.H
        // TODO // RAND.H
        // TODO // ROMIO.H
        // TODO // SETJMP.H
        // TODO // STDARG.H
        // TODO // STDDEF.H
        // TODO // STDIO.H
        // TODO // STDLIB.H
        // TODO // STRING.H
        // TODO // STRINGS.H
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
                break;
            case CXCursorKind.CXCursor_TypedefDecl:
                WriteLine($"{info} -> {cursor.TypedefDeclUnderlyingType}");
                list.Add(cursor);
                break;
        }

        return CXChildVisitResult.CXChildVisit_Continue;
    }
}