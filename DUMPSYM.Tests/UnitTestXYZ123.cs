using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Text;
using System.Text.RegularExpressions;
using DUMPSYM.Extensions;

// ReSharper disable RedundantIfElseBlock
// ReSharper disable StringLiteralTypo

namespace DUMPSYM.Tests;

[TestClass]
[SuppressMessage("ReSharper", "InconsistentNaming")]
public sealed class UnitTestXYZ123 : UnitTestBase
{
    private readonly Regex RegexNewLine = new(Environment.NewLine, RegexOptions.Compiled | RegexOptions.Multiline);

    private StringBuilder Builder { get; } = new();

    private HashSet<ISymbolDefinition> Typedefs { get; } = [];

    private HashSet<ISymbolDefinition2> TypedefsComplex { get; } = [];

    private HashSet<ISymbolDefinition> Types { get; } = [];

    [TestMethod]
    public void TestMethod1()
    {
        Builder.Clear();
        Typedefs.Clear();
        TypedefsComplex.Clear();
        var parse = Parse(Sample.Default.Symbols);
        File.WriteAllText(@"C:\Files\GitHub\! PSX\DUMPSYM\Project1\test.cpp", parse);
    }

    private string Parse(List<Symbol> symbols)
    {
        Builder.AppendLine("// ReSharper disable CppInconsistentNaming");
        Builder.AppendLine("// ReSharper disable CommentTypo");
        Builder.AppendLine("// ReSharper disable IdentifierTypo");
        Builder.AppendLine("// ReSharper disable CppClangTidyClangDiagnosticReservedIdentifier");
        Builder.AppendLine("// ReSharper disable CppClangTidyBugproneReservedIdentifier");

        var count = 0;

        var stopwatch = Stopwatch.StartNew();

        for (var index = 0; index < symbols.Count; index++)
        {
            var symbol = symbols[index];

            if (!Parse(symbol, ref index, symbols))
            {
                continue;
            }

            count++;
        }

        var elapsed = stopwatch.Elapsed;

        var output = Builder.ToString();

        Builder.Insert(0,
            $"// Symbols: {count} of {symbols.Count} ({(double)count / symbols.Count:P5}), " +
            $"Lines: {RegexNewLine.Matches(output).Count}, " +
            $"Time: {elapsed:g}{Environment.NewLine}");

        return Builder.ToString().TrimEnd();
    }

    private bool Parse(Symbol symbol, ref int symbolIndex, List<Symbol> symbols)
    {
        var record = symbol.Record;

        if (!ParseSimple(symbol, ref symbolIndex, symbols))
        {
            return false;
        }

        if (ParseType(ref symbolIndex, symbols, record))
        {
            return true;
        }

        if (ParseTypedef(symbol))
        {
            return true;
        }

        Builder.AppendLine($"// {symbol.ToString().ReplaceLineEndings(", ")}");

        return true;
    }

    private bool ParseSimple(Symbol symbol, ref int symbolIndex, List<Symbol> symbols)
    {
        var record = symbol.Record;

        if (record.IsLineModifier())
        {
            return false;
        }

        if (record.IsFileEnd())
        {
            return false;
        }

        if (record.IsFileHeader())
        {
            if (Log.File)
            {
                Builder.AppendLine($"// [FILE] {symbol}");
            }

            return false;
        }

        if (record.IsFunctionHeader())
        {
            var end = symbols.FindIndex(symbolIndex, s => s.Record.IsFunctionFooter());

            Assert.AreNotEqual(-1, end);

            symbolIndex = end;

            return false;
        }

        if (record.IsStatic())
        {
            return false;
        }

        if (record.IsExternal())
        {
            return false;
        }

        if (record.IsName())
        {
            return false;
        }

        return true;
    }

    private bool ParseType(ref int symbolIndex, List<Symbol> symbols, SymbolRecord record)
    {
        if (!record.IsType(out var type))
        {
            return false;
        }

        var start = symbolIndex;
        var end = symbols.FindIndex(symbolIndex, s => s.Record.IsTypeFooter(type));

        Assert.AreNotEqual(-1, end);

        symbolIndex = end;

        if (Types.Add(type))
        {
            if ((Log.Type && Log.TypeAdded) || Log.AnyAdded)
            {
                if (Log.Code)
                {
                    var extra = 0;

                    if (SymbolRegistry.HasFakeName(type.Name))
                    {
                        var index = end + 1;

                        if (index >= 0 && index < symbols.Count)
                        {
                            var symbol = symbols[index];

                            if (symbol.Record.IsTypedef2(out var tpd) && tpd.Tag == type.Name)
                            {
                                extra = 1;
                            }
                        }
                    }
                    else
                    {
                        extra = 0;
                    }

                    Builder.AppendLine(SymbolParserUtility.Parse(symbols[start..(end + 1 + extra)]).Trim());
                }
                else
                {
                    Builder.AppendLine($"// ADDED [TYPE]: {type}");
                }
            }

            return true;
        }
        else
        {
            if ((Log.Type && Log.TypeSkipped) || Log.AnySkipped)
            {
                Builder.AppendLine($"// SKIPPED [TYPE]: {type}");
            }

            return true;
        }
    }

    private bool ParseTypedef(Symbol symbol)
    {
        var record = symbol.Record;

        if (!record.IsTypedef(out var typedef1)) // TODO should check is not 2
        {
            return false;
        }

        if (!record.IsTypedef2(out var typedef2))
        {
            return ParseTypedef1(symbol, typedef1);
        }
        else
        {
            return ParseTypedef2(symbol, typedef2);
        }
    }

    private bool ParseTypedef1(Symbol symbol, ISymbolDefinition typedef1)
    {
        if (Typedefs.Add(typedef1))
        {
            if ((Log.Typedef && Log.Typedef1 && Log.Typedef1Added) || Log.AnyAdded)
            {
                if (Log.Code) // TODO skip size_t, wchar_t
                {
                    Builder.AppendLine(SymbolParserUtility.Parse([symbol]).Trim());
                }
                else
                {
                    Builder.AppendLine($"// ADDED [TYPEDEF1]: {typedef1}");
                }
            }
        }
        else
        {
            if ((Log.Typedef && Log.Typedef1 && Log.Typedef1Skipped) || Log.AnySkipped)
            {
                Builder.AppendLine($"// SKIPPED [TYPEDEF1]: {typedef1}");
            }
        }

        return true;
    }

    private bool ParseTypedef2(Symbol symbol, ISymbolDefinition2 typedef2)
    {
        if (TypedefsComplex.Add(typedef2))
        {
            if ((Log.Typedef && Log.Typedef2 && Log.Typedef2Added) || Log.AnyAdded)
            {
                if (Log.Code)
                {
                    Builder.AppendLine(SymbolParserUtility.Parse([symbol]).Trim());
                }
                else
                {
                    Builder.AppendLine($"// ADDED [TYPEDEF2]: {typedef2}");
                }
            }
        }
        else
        {
            if ((Log.Typedef && Log.Typedef2 && Log.Typedef2Skipped) || Log.AnySkipped)
            {
                Builder.AppendLine($"// SKIPPED [TYPEDEF2]: {typedef2}");
            }
        }

        return true;
    }

    private static class Log
    {
        public static bool Code { get; } = true;

        public static bool File { get; } = false;

        public static bool AnySkipped { get; } = false;

        public static bool AnyAdded { get; } = false;

        public static bool Type { get; } = true;

        public static bool TypeAdded { get; } = true;

        public static bool TypeSkipped { get; } = false;

        public static bool Typedef { get; } = true;

        public static bool Typedef1 { get; } = true;

        public static bool Typedef1Added { get; } = true;

        public static bool Typedef1Skipped { get; } = false; // TODO false

        public static bool Typedef2 { get; } = true;

        public static bool Typedef2Added { get; } = true;

        public static bool Typedef2Skipped { get; } = false; // TODO false
    }
}