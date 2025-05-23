using System.CodeDom.Compiler;
using System.Collections.Frozen;
using System.Collections.Immutable;
using System.Diagnostics.CodeAnalysis;
using System.Text.RegularExpressions;
using DUMPSYM.Extensions;

// ReSharper disable CommentTypo

// ReSharper disable RedundantIfElseBlock
// ReSharper disable IdentifierTypo
// ReSharper disable StringLiteralTypo

namespace DUMPSYM.Tests;

/*
 * TODO no more typedef struct w/ exceptions
 * TODO ignore some symbols TODO which ones?
 * TODO named should always be unique while fake not necessarily
 * TODO generation shall be done using typedefs so CliPt can exist
 */
public sealed class Generator : IDisposable
{
    [SuppressMessage("ReSharper", "ConvertToPrimaryConstructor")]
    public Generator(List<Symbol> symbols, GeneratorOptions? options = null)
    {
        Console.WriteLine($"symbols: {symbols.Count}");

        symbols = CleanupSymbols(symbols);

        Console.WriteLine($"symbols cleanup: {symbols.Count}");

        var split1 = Symbol.Split(symbols.ToArray());

        Console.WriteLine($"split: {split1.Length}");

        var split2 = split1.Distinct(SymbolArrayEqualityComparer.MembersTypeName).ToArray();

        Console.WriteLine($"split distinct: {split2.Length}");

        Split = split2.Where(s => s[0].IsTypeHeader || s[0].IsTypeDefinition).ToArray();

        Console.WriteLine($"type/typedef: {Split.Length}");

        symbols = Split.SelectMany(s => s).ToList();

        Console.WriteLine($"type/typedef raw: {symbols.Count}");

        MapForward = new Dictionary<Symbol, Symbol?>();

        MapReverse = new Dictionary<Symbol, Symbol>();

        var defs = Split.Select(s => s[0]).Where(s => s.IsTypeDefinition).ToArray();

        foreach (var def in defs)
        {
            MapForward[def] = null;

            if (def.Tag == null)
            {
                continue;
            }

            for (var i = symbols.IndexOf(def) - 1; i >= 0; i--)
            {
                var symbol = symbols[i];

                if (symbol.Tag != def.Tag)
                {
                    continue;
                }

                MapForward[def] = symbol;
                MapReverse[symbol] = def;
                break;
            }
        }

        Symbols = symbols.ToImmutableList();

        SymbolsSplit = Symbol.Split(symbols.ToArray());

        Definitions = symbols.Where(s => s.IsTypeDefinition).Distinct(Symbol.RecordEqualityComparer).ToImmutableList();

        Console.WriteLine(Definitions.Count);

        var files = new Dictionary<Symbol, Symbol>();

        var file = default(Symbol);

        foreach (var symbol in Symbols)
        {
            if (symbol.IsFile)
            {
                file = symbol;
            }

            files.Add(symbol, file!);
        }

        SymbolsFiles = files.ToFrozenDictionary();

        SymbolsIndices = symbols.Index().ToFrozenDictionary(s => s.Item, s => s.Index);

        var dictionary = new Dictionary<Symbol, string>();

        foreach (var symbol in Symbols.Where(s => s.IsTypeDefinition || s.IsTypeHeader))
        {
            var s = symbol.Name!;

            var t = RegexFakeName.IsMatch(s) ? $"_{s[1..]}{symbol.Header.Position:x6}" : s;

            dictionary.Add(symbol, t);
        }

        SymbolsNames = dictionary.ToFrozenDictionary();

        Options = options ?? new GeneratorOptions();
    }

    private Dictionary<Symbol, Symbol?> MapForward { get; } // TODO use

    private Dictionary<Symbol, Symbol> MapReverse { get; } // TODO use

    private Symbol[][] Split { get; } // TODO use

    [Obsolete]
    private Symbol[][] SymbolsSplit { get; }

    [Obsolete]
    private ImmutableList<Symbol> Definitions { get; }

    private ImmutableList<Symbol> Symbols { get; }

    [Obsolete]
    private FrozenDictionary<Symbol, Symbol> SymbolsFiles { get; }

    private FrozenDictionary<Symbol, int> SymbolsIndices { get; }

    private FrozenDictionary<Symbol, string> SymbolsNames { get; }

    private GeneratorCounters Counters { get; } = new();

    private GeneratorOptions Options { get; }

    private GeneratorSets Sets { get; } = new();

    private IndentedTextWriter Writer { get; } = new(new StringWriter());

    private static Regex RegexFakeName { get; } = new(@"^\.\d+fake$", RegexOptions.Compiled | RegexOptions.CultureInvariant);

    private static Regex RegexNewLine { get; } = new(@"\r?\n", RegexOptions.Compiled | RegexOptions.CultureInvariant);

    public void Dispose()
    {
        Writer.Dispose();
    }

    public static List<Symbol> CleanupSymbols(List<Symbol> symbols)
    {
        var types1 = Enum.GetValues<SymbolTypeKind>().Where(IsPrimitive).Select(s => new SymbolType(s)).ToArray();

        var types2 = types1.Select(s => new SymbolType(s.Kind, SymbolTypeModifier.PTR)).ToArray();

        Remove(symbols, types1);

        Remove(symbols, types2);

        var types3 = new[]
            {
                new SymbolRecordDef(SymbolStorageClass.TPDEF, new SymbolType(SymbolTypeKind.UCHAR), 0, "u_char"),
                new SymbolRecordDef(SymbolStorageClass.TPDEF, new SymbolType(SymbolTypeKind.USHORT), 0, "u_short"),
                new SymbolRecordDef(SymbolStorageClass.TPDEF, new SymbolType(SymbolTypeKind.UINT), 0, "u_int"),
                new SymbolRecordDef(SymbolStorageClass.TPDEF, new SymbolType(SymbolTypeKind.ULONG), 0, "u_long")
            }
            .Select(s => new Symbol(new SymbolHeader { Type = 0x94 }, s)).ToArray();

        var index = symbols.FindIndex(s => s.IsFileEnd);

        symbols.InsertRange(index + 1, types3);

        return symbols;

        static int Remove(List<Symbol> symbols, SymbolType[] types)
        {
            return symbols.RemoveAll(s => s.IsTypeDefinition && types.Contains(s.Type!.Value));
        }
    }

    private static bool IsPrimitive(SymbolTypeKind kind)
    {
        return kind switch
        {
            SymbolTypeKind.NULL   => false,
            SymbolTypeKind.VOID   => false,
            SymbolTypeKind.CHAR   => true,
            SymbolTypeKind.SHORT  => true,
            SymbolTypeKind.INT    => true,
            SymbolTypeKind.LONG   => true,
            SymbolTypeKind.FLOAT  => true,
            SymbolTypeKind.DOUBLE => true,
            SymbolTypeKind.STRUCT => false,
            SymbolTypeKind.UNION  => false,
            SymbolTypeKind.ENUM   => false,
            SymbolTypeKind.MOE    => false,
            SymbolTypeKind.UCHAR  => true,
            SymbolTypeKind.USHORT => true,
            SymbolTypeKind.UINT   => true,
            SymbolTypeKind.ULONG  => true,
            _                     => throw new ArgumentOutOfRangeException(nameof(kind), kind, null)
        };
    }

    public string? Generate(Symbol[] source)
    {
        return Generate();
        return null; // TODO
        Writer.WriteLine("#pragma once");
        Writer.WriteLine("// ReSharper disable CommentTypo");
        Writer.WriteLine("// ReSharper disable CppClangTidyBugproneReservedIdentifier");
        Writer.WriteLine("// ReSharper disable CppClangTidyClangDiagnosticReservedIdentifier");
        Writer.WriteLine("// ReSharper disable CppInconsistentNaming");
        Writer.WriteLine("// ReSharper disable IdentifierTypo");

        foreach (var symbols in SymbolsSplit)
        {
            var header = symbols[0];

            if (header.IsFile)
            {
                GenerateFile(header);
            }
            else if (header.IsTypeHeader)
            {
                GenerateType(header, symbols);
            }
            else if (header.IsTypeDefinition)
            {
                GenerateTypedef(header);
            }
            else if (header.IsExternal)
            {
                GenerateDefault(header);
            }
            else if (header.IsFileEnd)
            {
                GenerateDefault(header);
            }
            else if (header.IsFunction)
            {
                GenerateDefault(header);
            }
            else if (header.IsStatic)
            {
                GenerateDefault(header);
            }
            else if (header.IsVariable)
            {
                // GenerateDefault(header); // BUG/TODO these appear in last file, that's wrong
            }
            else
            {
                throw new NotImplementedException(header.ToString());
            }
        }

        return Writer.InnerWriter.ToString();
    }

    private string? Generate()
    {
        var set = new HashSet<Symbol>();
        /*
         * TODO
         * generate split3
         * if it's a type, grab eos, if map reverse doesn't contain eos key, it's a compiler type to generate
         */

        foreach (var symbols in Split)
        {
            var header = symbols[0];

            if (set.Contains(header))
            {
                // Console.WriteLine($"{set.Count} {hdr}"); // TODO delete
                continue;
            }

            if (header.ToString() == "00033e: $00000000 94 Def class STRTAG type STRUCT size 44 name LoadFiles")
            {
                _ = 0;
            }

            if (header.IsTypeDefinition)
            {
                if (MapForward.TryGetValue(header, out var value))
                {
                    if (header.HasFakeTag)
                    {
                        if (value == null)
                        {
                            throw new InvalidOperationException();
                        }
                        else
                        {
                            if (value.IsTypeDefinition)
                            {
                                var z = 0;
                                //Console.WriteLine(header);
                                //Console.WriteLine(value);
                                // 0d3ed6: $00000000 96 Def2 class TPDEF type STRUCT size 16 dims 0 tag .95fake name CliPt // TODO MapForward[value] (CliPt -> EngineCoord -> .95fake)
                            }
                            else
                            {
                                throw new InvalidOperationException();
                            }
                        }
                    }
                    else
                    {
                        if (value == null)
                        {
                            var z = 0;
                            // 000000: $00000000 94 Def class TPDEF type UCHAR size 0 name u_char
                            // 000000: $00000000 94 Def class TPDEF type USHORT size 0 name u_short
                            // 000000: $00000000 94 Def class TPDEF type UINT size 0 name u_int
                            // 000000: $00000000 94 Def class TPDEF type ULONG size 0 name u_long
                            // 00609d: $00000000 94 Def class TPDEF type PTR FCN VOID size 0 name SpuIRQCallbackProc
                            // 0060bd: $00000000 94 Def class TPDEF type PTR FCN VOID size 0 name SpuTransferCallbackProc
                            // 0060e2: $00000000 94 Def class TPDEF type PTR FCN VOID size 0 name SpuStCallbackProc

                            // Console.WriteLine(header);

                            var modifiers = header.Type!.Value.Modifiers.ToArray();

                            if (modifiers.Any(s => s is SymbolTypeModifier.FCN))
                            {
                                Console.WriteLine($"typedef void (*{header.Name})();");
                            }
                            else
                            {
                                var s1 = SymbolGenerator.ToString(header.Type.Value.Kind);
                                var s3 = new string('*', modifiers.Count(s => s is SymbolTypeModifier.PTR));
                                var s5 = string.Concat((header.Dimensions ?? []).Select(s => $"[{s}]"));
                                Console.WriteLine($"typedef {s1}{s3} {header.Name}{s5};");
                            }
                        }
                        else
                        {
                            if (value.IsTypeDefinition)
                            {
                                var z = 0;
                                // 0004ca: $00000000 96 Def2 class TPDEF type PTR STRUCT size 8 dims 0 tag Sprite name SpritePtr
                            }
                            else
                            {
                                var z = 0;
                                // Console.WriteLine(hdr);
                                // 006bc7: $00000000 96 Def2 class TPDEF type STRUCT size 16 dims 0 tag HeapHandle name HeapHandle
                                // 006e8d: $00000000 96 Def2 class TPDEF type STRUCT size 28 dims 0 tag AnimPageDynamic name AnimPageDynamic
                            }
                        }
                    }
                }
                else
                {
                    throw new InvalidOperationException();
                }

                continue;
            }

            if (header.IsTypeHeader)
            {
                var footer = symbols[^1];

                var kind = SymbolGenerator.ToString(header.Type!.Value.Kind);

                if (MapReverse.TryGetValue(footer, out var symbol)) // TODO generate fake struct using typedef name
                {
                    set.Add(symbol);

                    if (header.HasFakeName)
                    {
                        var z = 0;
                        Writer.WriteLine($"{kind} {symbol.Name}; // {header} // {symbol}");
                    }
                    else
                    {
                        var z = 0;
                        Writer.WriteLine($"{kind} {header.Name}; // {header}");

                        if (symbol.Name != symbol.Tag)
                        {
                            Writer.WriteLine(
                                $"{SymbolGenerator.ToString(SymbolStorageClass.TPDEF)} {kind} {symbol.Tag}{new string('*', symbol.Type!.Value.Modifiers.Count(s => s is SymbolTypeModifier.PTR))} {symbol.Name}; // {symbol}");
                        }
                    }
                }
                else
                {
                    if (header.HasFakeName)
                    {
                        var z = 0;
                        Writer.WriteLine($"{kind} {SymbolsNames[header]}; // {header}");
                    }
                    else
                    {
                        var z = 0;
                        Writer.WriteLine($"{kind} {SymbolsNames[header]}; // {header}");
                    }
                }

                continue;
            }

            throw new InvalidOperationException(header.ToString());
        }

        return Writer.InnerWriter.ToString();
    }

    private void GenerateDefault(Symbol header)
    {
        return;
        var split = RegexNewLine.Split(header.ToString());

        var join = string.Join(", ", split.Select(s => s.Trim()));

        Writer.WriteLine($"// TODO: {join}");
    }

    private void GenerateFile(Symbol header)
    {
        return;
        Writer.WriteLine($"// FILE: {header}");
    }

    private void GenerateType(Symbol header, Symbol[] symbols)
    {
        if (!header.IsTypeHeader)
        {
            throw new ArgumentOutOfRangeException(nameof(header), header, null);
        }

        if (!Options.GenerateTypes)
        {
            return;
        }

        var footer = symbols[^1];

        var counter = Counters.Types;

        counter.Total++;

        var name = header.Name!;

        if (name is "tm") // TODO
        {
            Writer.WriteLine(GetIgnored(header));

            counter.Ignored++;

            return;
        }

        if (Sets.Types.Add(symbols))
        {
            var klass = SymbolGenerator.ToString(header.Class!.Value);

            var b = TryGetDefinition(header, out var def);

            if (false && b) // TODO delete
            {
                var typedef = SymbolGenerator.ToString(SymbolStorageClass.TPDEF);

                if (header.HasFakeName || def.Tag == def.Name)
                {
                    Writer.WriteLine($"{typedef} {klass} {{ // {header}");
                }
                else
                {
                    Writer.WriteLine($"{typedef} {klass} {name} {{ // {header}");
                }

                using (Writer.GetIndentScope())
                {
                    var members = symbols[1..^1];

                    foreach (var member in members) // TODO
                    {
                        Writer.WriteLine($"int {member.Name}; // {member}");
                    }
                }

                Writer.WriteLine($"}} {new string('*', def.Type!.Value.Modifiers.Count(s => s is SymbolTypeModifier.PTR))}{def.Name}; // {def}");
            }
            else // HL
            {
                var typeName = SymbolsNames[header]; // TODO

                typeName = header.Name;

                if (header.HasFakeName)
                {
                    typeName = def?.Name ?? SymbolsNames[header] ?? header.Name;
                }

                Writer.WriteLine($"{klass} {typeName} {{ // {header}");

                using (Writer.GetIndentScope())
                {
                    var members = symbols[1..^1];

                    foreach (var member in members) // TODO
                    {
                        Writer.WriteLine(GetMemberSignature(member, header));
                    }
                }

                Writer.WriteLine($"}}; // {footer}");
            }

            counter.Added++;
        }
        else
        {
            counter.Ignored++;
        }
    }

    private string GetMemberSignature(Symbol member, Symbol header)
    {
        // TODO ARY/FCN/PTR

        var type = member.Type!.Value;

        var ptrs = new string('*', type.Modifiers.Count(s => s is SymbolTypeModifier.PTR));

        var dims = string.Concat((member.Dimensions ?? []).Select(s => $"[{s}]"));

        var kind = type.Kind;

        var find = Definitions.Find(s => s.Type!.Value.Kind == kind && !s.Type.Value.Modifiers.Any());

        var tag = member.Tag;
        var name = find?.Name ?? SymbolGenerator.ToString(kind);

        if (member.ToString() == "0d404e: $00000010 96 Def2 class MOS type ARY STRUCT size 144 dims 1 9 tag .95fake name BPoints")
        {
            var z = 0;
        }

        if (string.IsNullOrEmpty(tag))
        {
            return $"/* case 1 */ {name}{ptrs} {member.Name}{dims}; // {member}";
        }
        else
        {
            if (member.HasFakeTag)
            {
                var typeName = default(string);

                var headerIndex = SymbolsIndices[header];

                for (var i = headerIndex - 1; i >= 0; i--)
                {
                    var current = Symbols[i];

                    if (current.IsTypeDefinition && current.Tag == tag)
                    {
                        typeName = current.Name!;
                        break;
                    }
                }

                if (typeName == null)
                {
                    for (var i = headerIndex - 1; i >= 0; i--)
                    {
                        var current = Symbols[i];

                        if (current.IsTypeHeader && current.Name == tag)
                        {
                            typeName = current.Name!;
                            typeName = SymbolsNames[current];
                            break;
                        }
                    }
                }

                // BUG: CliPt is not generated, it's like EngineCoord
                Assert.IsNotNull(typeName, member.ToString());
                return $"/* case 2 */ {SymbolGenerator.ToString(kind)} {typeName}{ptrs} {member.Name}{dims}; // {member}";

                return member.ToString();
                throw new NotImplementedException(member.ToString());
            }
            else
            {
                return $"/* case 3 */ {SymbolGenerator.ToString(kind)} {tag}{ptrs} {member.Name}{dims}; // {member}";

                throw new NotImplementedException(member.ToString());
            }

            if (kind is SymbolTypeKind.STRUCT or SymbolTypeKind.UNION)
            {
                var typeName = default(string);

                if (member.HasFakeTag)
                {
                    var headerIndex = SymbolsIndices[header];

                    for (var i = headerIndex - 1; i >= 0; i--)
                    {
                        var current = Symbols[i];

                        if (current.IsTypeDefinition && current.Tag == tag)
                        {
                            typeName = current.Name!;
                            break;
                        }
                    }
                }
                else
                {
                    typeName = tag;
                }

                Assert.IsNotNull(typeName, member.ToString());

                return $"/* case 2 */ {SymbolGenerator.ToString(kind)} {typeName}{ptrs} {member.Name}{dims}; // {member}";
                return $"/* case 2 */ {SymbolGenerator.ToString(kind)} {find?.Name}{ptrs} {member.Name}{dims}; // {member}";
                return $"/* case 2 */ {SymbolGenerator.ToString(kind)} {tag}{ptrs} {member.Name}{dims}; // {member}";
                return $"/* case 2 */ {SymbolGenerator.ToString(kind)} {name}{ptrs} {member.Name}{dims}; // {member}";
            }
            else
            {
                return $"/* case 3 */ {name}{ptrs} {member.Name}{dims}; // {member}";
            }
        }
    }

    private void GenerateTypedef(Symbol header)
    {
        if (!Options.GenerateTypedefs)
        {
            return;
        }

        var counter = Counters.Typedefs;

        counter.Total++;

        var name = header.Name!;

        if (name is "clock_t" or "size_t" or "wchar_t" or "ushort")
        {
            Writer.WriteLine(GetIgnored(header));

            counter.Ignored++;

            return;
        }

        if (Sets.Typedefs.Add(header))
        {
            var type = header.Type!.Value;

            var kind = SymbolGenerator.ToString(type.Kind);

            var modifiers = type.Modifiers.ToArray();

            var pointers = new string('*', modifiers.Count(s => s is SymbolTypeModifier.PTR));

            var fcn = modifiers.Any(s => s is SymbolTypeModifier.FCN);

            var ptr = modifiers.Any(s => s is SymbolTypeModifier.PTR);

            var tag = header.Tag;

            if (tag == null)
            {
                if (fcn)
                {
                    Writer.WriteLine($"typedef {kind} ({pointers}{name})(); // {header}");
                }
                else
                {
                    Writer.WriteLine($"typedef {kind} {pointers}{name}; // {header}");
                }
            }
            else
            {
                if (fcn)
                {
                    throw new NotImplementedException(header.ToString()); // none thus far
                }

                if (type.Kind is SymbolTypeKind.STRUCT or SymbolTypeKind.UNION)
                {
                    if (ptr)
                    {
                        var previous = Symbols[SymbolsIndices[header] - 1];

                        if (previous.Class is SymbolStorageClass.EOS && previous.Tag == tag)
                        {
                            Writer.WriteLine($"// IGNORED: typedef struct // {header}");
                            counter.Added--;
                            counter.Ignored++;
                        }
                        else
                        {
                            Writer.WriteLine($"typedef {kind} {tag} {pointers}{name}; // {header}");
                        }
                    }
                    else
                    {
                        Writer.WriteLine($"// IGNORED: typedef struct // {header}");
                        counter.Added--;
                        counter.Ignored++;
                    }
                }
                else
                {
                    throw new NotImplementedException(header.ToString()); // none thus far
                }
            }

            counter.Added++;
        }
        else
        {
            counter.Ignored++;
        }
    }

    private string GetIgnored(Symbol symbol)
    {
        return $"// IGNORED: {symbol}, SOURCE: {SymbolsFiles[symbol]}";
    }

    public string GetStatistics()
    {
        return Counters.ToString();
    }

    private bool TryGetDefinition(Symbol type, [MaybeNullWhen(false)] out Symbol result)
    {
        result = null;

        if (!type.IsTypeHeader)
        {
            throw new ArgumentOutOfRangeException(nameof(type), type, null);
        }

        var hdr = SymbolsIndices[type];

        var eos = Symbols.FindIndex(hdr, s => s.IsTypeFooter);

        var def = Symbols[eos + 1];

        if (def.IsTypeDefinition && def.Tag == type.Name)
        {
            result = def;
        }

        return result != null;
    }

    private sealed class GeneratorSets
    {
        public HashSet<Symbol[]> Types { get; } = new(SymbolArrayEqualityComparer.MembersTypeName);

        public HashSet<Symbol> Typedefs { get; } = new(Symbol.RecordEqualityComparer);
    }

    private sealed class GeneratorCounter
    {
        public int Added { get; set; }

        public int Ignored { get; set; }

        public int Total { get; set; }

        public void Check()
        {
            if (Added + Ignored != Total)
            {
                throw new InvalidOperationException($"Added + Ignored != Total: {this}");
            }
        }

        public override string ToString()
        {
            return $"{nameof(Added)}: {Added}, {nameof(Ignored)}: {Ignored}, {nameof(Total)}: {Total}";
        }
    }

    private sealed class GeneratorCounters
    {
        public GeneratorCounter Typedefs { get; } = new();

        public GeneratorCounter Types { get; } = new();

        public override string ToString()
        {
            Typedefs.Check();

            Types.Check();

            using var writer = new StringWriter();

            writer.WriteLine($"Typedefs: {Typedefs}");

            writer.WriteLine($"Types: {Types}");

            var s = writer.ToString();

            return s;
        }
    }
}

public sealed class GeneratorOptions
{
    public bool GenerateTypedefs { get; } = true;

    public bool GenerateTypes { get; } = true;
}