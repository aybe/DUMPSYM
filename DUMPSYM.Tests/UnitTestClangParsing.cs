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
            "-D", "LANGUAGE_C", // KERNEL.H // BUG these have no typedefs
            "-D", "_SIZE_T",    // typedef redefinition with different types ('unsigned int' vs 'unsigned long long'): Line 69, Column 22 in SYS/TYPES.H
            "-D", "_WCHAR_T"    // 'long wchar_t' is invalid: Line 19, Column 18 in STDDEF.H
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
        _ = 0;                           // ABS.H           // BUG useless 
        _ = 0;                           // ASM.H           // BUG useless 
        _ = 0;                           // ASSERT.H        // BUG useless 
        _ = 0;                           // CONVERT.H       // BUG useless 
        _ = 0;                           // CTYPE.H         // BUG useless 
        yield return [Headers.FS];       // FS.H
        _ = 0;                           // GTEMAC.H        // BUG useless 
        _ = 0;                           // GTENOM.H        // BUG useless 
        _ = 0;                           // GTEREG_S.H      // BUG useless 
        _ = 0;                           // GTEREG.H        // BUG useless 
        _ = 0;                           // INLINE_A.H      // BUG useless 
        _ = 0;                           // INLINE_C.H      // BUG useless 
        _ = 0;                           // INLINE_O.H      // BUG useless 
        _ = 0;                           // INLINE_S.H      // BUG useless 
        yield return [Headers.KERNEL];   // KERNEL.H
        _ = 0;                           // LIBAPI.H        // BUG useless 
        yield return [Headers.LIBCD];    // LIBCD.H
        _ = 0;                           // LIBCOMB.H       // BUG useless 
        yield return [Headers.LIBDS];    // LIBDS.H
        _ = 0;                           // LIBETC.H        // BUG useless 
        yield return [Headers.LIBGPU];   // LIBGPU.H
        yield return [Headers.LIBGS];    // LIBGS.H
        yield return [Headers.LIBGTE];   // LIBGTE.H
        _ = 0;                           // LIBGUN.H        // BUG useless 
        yield return [Headers.LIBHMD];   // LIBHMD.H
        _ = 0;                           // LIBMATH.H       // BUG useless 
        yield return [Headers.LIBMCRD];  // LIBMCRD.H
        _ = 0;                           // LIBMCX.H        // BUG useless 
        _ = 0;                           // LIBPAD.H        // BUG useless 
        yield return [Headers.LIBPRESS]; // LIBPRESS.H
        _ = 0;                           // LIBSIO.H        // BUG useless 
        _ = 0;                           // LIBSN.H         // BUG useless 
        yield return [Headers.LIBSND];   // LIBSND.H
        yield return [Headers.LIBSPU];   // LIBSPU.H
        _ = 0;                           // LIBTAP.H        // BUG useless 
        _ = 0;                           // LIMITS.H        // BUG useless 
        _ = 0;                           // MALLOC.H        // BUG useless 
        yield return [Headers.MCGUI];    // MCGUI.H
        _ = 0;                           // MEMORY.H        // BUG useless 
        _ = 0;                           // QSORT.H         // BUG useless 
        _ = 0;                           // R3000.H         // BUG useless 
        _ = 0;                           // RAND.H          // BUG useless 
        _ = 0;                           // ROMIO.H         // BUG useless 
        yield return [Headers.SETJMP];   // SETJMP.H
        yield return [Headers.STDARG];   // STDARG.H
        yield return [Headers.STDDEF];   // STDDEF.H
        _ = 0;                           // STDIO.H         // BUG useless 
        _ = 0;                           // STDLIB.H        // BUG useless 
        _ = 0;                           // STRING.H        // BUG useless 
        _ = 0;                           // STRINGS.H       // BUG useless 
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
}