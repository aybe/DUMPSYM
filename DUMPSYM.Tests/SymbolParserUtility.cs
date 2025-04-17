#define FIX_FAKE_NAME

using System.CodeDom.Compiler;
using System.Diagnostics.CodeAnalysis;
using DUMPSYM.Extensions;

namespace DUMPSYM.Tests;

public static class SymbolParserUtility
{
    private const int Padding = 55;

    public static string Parse(List<Symbol> symbols)
    {
        var parsers = new SymbolParser[]
        {
            new SymbolParserFile(),
            new SymbolParserFileEnd(),
            new SymbolParserFunction(),
            new SymbolParserType(),
            new SymbolParserTypedef()
        };

        using var writer = GetWriter();

        var list = new LinkedList<Symbol>(symbols);

        for (var node = list.First; node != null; node = node.Next)
        {
            var wrong = node.Value.ToString().ReplaceLineEndings("\t");

            var parse = false;

            foreach (var parser in parsers)
            {
                parse = parser.TryParse(ref node, out var result);

                if (!parse)
                {
                    continue;
                }

                var output = string.IsNullOrWhiteSpace(result)
                    ? $"{"// WARNING: EMPTY RESULT",-Padding}// {wrong}"
                    : result;

                writer.WriteLine(output);

                break;
            }

            if (parse)
            {
                continue;
            }

            writer.WriteLine($"{"// ERROR: NOT IMPLEMENTED",-Padding}// {wrong}");
        }

        return writer.InnerWriter.ToString()!;
    }


    private static string GetString(SymbolStorageClass value)
    {
        // ReSharper disable once SwitchExpressionHandlesSomeKnownEnumValuesWithExceptionInDefault
        return value switch
        {
            SymbolStorageClass.ENTAG  => "enum",
            SymbolStorageClass.STRTAG => "struct",
            SymbolStorageClass.UNTAG  => "union",
            _                         => throw new ArgumentOutOfRangeException(nameof(value), value, null)
        };
    }

    private static IndentedTextWriter GetWriter()
    {
        return new IndentedTextWriter(new StringWriter());
    }

    private abstract class SymbolParser
    {
        public abstract bool TryParse(ref LinkedListNode<Symbol> node, [MaybeNullWhen(false)] out string result);
    }

    private sealed class SymbolParserFile : SymbolParser
    {
        public override bool TryParse(ref LinkedListNode<Symbol> node, [MaybeNullWhen(false)] out string result)
        {
            result = null;

            if (node.Value.Record is not ISymbolFileStart)
            {
                return false;
            }

            while (true)
            {
                node = node.Next ?? throw new InvalidOperationException();

                if (node.Value.Record is ISymbolFileEnd)
                {
                    break;
                }
            }

            result = ""; // TODO

            return true;
        }
    }

    private sealed class SymbolParserFileEnd : SymbolParser
    {
        public override bool TryParse(ref LinkedListNode<Symbol> node, [MaybeNullWhen(false)] out string result)
        {
            result = null;

            if (node.Value.Record is not ISymbolFileEnd)
            {
                return false;
            }

            result = ""; // TODO

            return true;
        }
    }

    private sealed class SymbolParserFunction : SymbolParser
    {
        public override bool TryParse(ref LinkedListNode<Symbol> node, [MaybeNullWhen(false)] out string result)
        {
            result = null;

            if (node.Value.Record is not ISymbolFunction)
            {
                return false;
            }

            while (true)
            {
                node = node.Next ?? throw new InvalidOperationException();

                if (node.Value.Record is ISymbolFunctionEnd)
                {
                    break;
                }
            }

            result = ""; // TODO

            return true;
        }
    }

    private sealed class SymbolParserType : SymbolParser
    {
        public override bool TryParse(ref LinkedListNode<Symbol> node, [MaybeNullWhen(false)] out string result)
        {
            result = null;

            if (node.Value.Record is not ISymbolDefinition { Class: SymbolStorageClass.STRTAG or SymbolStorageClass.UNTAG } def)
            {
                return false;
            }

            result = Parse(ref node, def);

            return true;
        }

        private static string Parse(ref LinkedListNode<Symbol> node, ISymbolDefinition def)
        {
            using var writer = GetWriter();

            writer.Write($"{GetString(def.Class)} {SymbolRegistry.GetSafeName(def.Name)}".PadRight(Padding)); // TODO parse typedef
            writer.Write($"// {node.Value}");
            writer.WriteLine();

            writer.WriteLine("{");

            using (var _ = writer.GetIndentScope())
            {
                while (true)
                {
                    node = node.Next ?? throw new InvalidOperationException();

                    if (node.Value.Record is ISymbolDefinition2 { Class: SymbolStorageClass.EOS, Name: ".eos" })
                    {
                        break;
                    }
                    // TODO parse members, either MOS or MOU
                }
            }

            writer.Write("};");

            return writer.InnerWriter.ToString()!;
        }
    }

    private sealed class SymbolParserTypedef : SymbolParser
    {
        public override bool TryParse(ref LinkedListNode<Symbol> node, [MaybeNullWhen(false)] out string result)
        {
            result = null;

            if (node.Value.Record is not ISymbolDefinition { Class: SymbolStorageClass.TPDEF } def)
            {
                return false;
            }

            result = Parse(node.Value, def);

            return true;
        }

        private static string Parse(Symbol symbol, ISymbolDefinition def)
        {
            using var writer = GetWriter();

            var type = TypedefUtility.GetKindString(def.Type.Kind);

            var pointers = new string('*', def.Type.Modifiers.Count(s => s == SymbolTypeModifier.PTR));

            if (def.Type.Modifiers.Contains(SymbolTypeModifier.FCN))
            {
                writer.Write($"typedef {type} ({pointers}{def.Name})();".PadRight(Padding));
            }
            else
            {
                if (def is ISymbolDefinition2 def1)
                {
                    var tag = def1.Tag;

#if FIX_FAKE_NAME
                    if (tag.StartsWith('.'))
                    {
                        tag = $"_{tag[1..]}";
                    }
#endif

                    writer.Write($"typedef {type} {tag}{pointers} {def.Name};".PadRight(Padding));
                }
                else
                {
                    writer.Write($"typedef {type}{pointers} {def.Name};".PadRight(Padding));
                }
            }

            writer.Write($"// {symbol}");

            return writer.InnerWriter.ToString()!;
        }
    }
}