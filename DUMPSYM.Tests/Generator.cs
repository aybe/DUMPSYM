using System.CodeDom.Compiler;
using System.Collections.Frozen;
using System.Collections.Immutable;
using System.Diagnostics.CodeAnalysis;
using System.Text.RegularExpressions;
using DUMPSYM.Tests.WorkInProgress;

namespace DUMPSYM.Tests;

public sealed class Generator : IDisposable
{
    public Generator(string directory, GeneratorOptions? options = null)
    {
        Directory = directory;

        Options = options ?? new GeneratorOptions();
    }

    private string Directory { get; }

    private ImmutableList<Symbol> Symbols { get; set; }

    private FrozenDictionary<Symbol, Symbol> SymbolsFiles { get; set; } = null!;

    private FrozenDictionary<Symbol, int> SymbolsIndices { get; set; } = null!;

    private FrozenDictionary<Symbol, string> SymbolsNames { get; set; } = null!;

    private GeneratorCounters Counters { get; } = new();

    private GeneratorOptions Options { get; }

    private GeneratorSets Sets { get; } = new();

    private Dictionary<string, IndentedTextWriter> Writers { get; } = new();

    private static Regex RegexFakeName { get; } = new(@"^\.\d+fake$", RegexOptions.Compiled | RegexOptions.CultureInvariant);

    private static Regex RegexNewLine { get; } = new(@"\r?\n", RegexOptions.Compiled | RegexOptions.CultureInvariant);

    public void Dispose()
    {
        foreach (var writer in Writers.Values)
        {
            writer.Dispose();
        }
    }

    public void Generate(Symbol[] source)
    {
        Symbols = [..source];

        Initialize();

        SymbolsIndices = source.Index().ToFrozenDictionary(s => s.Item, s => s.Index);

        var dictionary = new Dictionary<Symbol, string>();

        foreach (var symbol in Symbols.Where(s => s.IsTypeDefinition || s.IsTypeHeader))
        {
            var s = symbol.Name!;

            var t = RegexFakeName.IsMatch(s) ? $"_{s[1..]}{symbol.Header.Position:x6}" : s;

            dictionary.Add(symbol, t);
        }

        SymbolsNames = dictionary.ToFrozenDictionary();

        var split = Symbol.Split(source);

        foreach (var symbols in split)
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
    }

    private void GenerateDefault(Symbol header)
    {
        var writer = GetWriter(header);

        var split = RegexNewLine.Split(header.ToString());

        var join = string.Join(", ", split.Select(s => s.Trim()));

        writer.WriteLine($"// TODO: {join}");
    }

    private void GenerateFile(Symbol header)
    {
        var writer = GetWriter(header);

        writer.WriteLine($"// FILE: {header}");
    }

    private void GenerateType(Symbol type, Symbol[] symbols)
    {
        if (!type.IsTypeHeader)
        {
            throw new ArgumentOutOfRangeException(nameof(type), type, null);
        }

        if (!Options.GenerateTypes)
        {
            return;
        }

        var counter = Counters.Types;

        counter.Total++;

        var writer = GetWriter(type);

        var name = type.Name!;

        if (name is "tm")
        {
            writer.WriteLine(GetIgnored(type));

            counter.Ignored++;

            return;
        }

        if (Sets.Types.Add(symbols))
        {
            writer.WriteLine($"{SymbolGenerator.ToString(type.Class!.Value)} {SymbolsNames[type]}; // {type}");

            counter.Added++;
        }
        else
        {
            counter.Ignored++;
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

        var writer = GetWriter(header);

        var name = header.Name!;

        if (name is "clock_t" or "size_t" or "wchar_t" or "ushort")
        {
            writer.WriteLine(GetIgnored(header));

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
                    writer.WriteLine($"typedef {kind} ({pointers}{name})(); // {header}");
                }
                else
                {
                    writer.WriteLine($"typedef {kind} {pointers}{name}; // {header}");
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
                        writer.WriteLine($"typedef {kind} {tag} {pointers}{name}; // {header}");
                    }
                    else
                    {
                        writer.WriteLine($"// IGNORED: typedef struct // {header}");
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

    private IndentedTextWriter GetWriter(string path)
    {
        if (!Writers.TryGetValue(path, out var writer))
        {
            Writers.Add(path, writer = new IndentedTextWriter(new StringWriter()));
        }

        return writer;
    }

    private IndentedTextWriter GetWriter(Symbol symbol) // TODO improve
    {
        string path;

        if (symbol.IsTypeHeader)
        {
            if (SdkMatch.TypesMap.TryGetValue(symbol.Name!, out path!))
            {
            }
            else
            {
                path = SymbolsFiles[symbol].File!;
            }
        }
        else if (symbol.IsTypeDefinition)
        {
            if (SdkMatch.DefinitionsMap.TryGetValue(symbol.Name!, out path!))
            {
            }
            else
            {
                path = SymbolsFiles[symbol].File!;
            }
        }
        else
        {
            path = SymbolsFiles[symbol].File!;
        }

        path = Path.GetFileName(path);

        path = Path.ChangeExtension(path, ".H");

        var writer = GetWriter(path);

        return writer;
    }

    private void Initialize()
    {
        var dictionary = new Dictionary<Symbol, Symbol>();

        var file = default(Symbol);

        foreach (var symbol in Symbols)
        {
            if (symbol.IsFile)
            {
                file = symbol;
            }

            if (file == null)
            {
                throw new InvalidOperationException();
            }

            dictionary.Add(symbol, file);
        }

        SymbolsFiles = dictionary.ToFrozenDictionary();
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

    public void Write()
    {
        const string header = // TODO external
            """
            #pragma once
            // ReSharper disable CommentTypo
            // ReSharper disable CppInconsistentNaming
            // ReSharper disable CppClangTidyBugproneReservedIdentifier
            // ReSharper disable CppClangTidyClangDiagnosticReservedIdentifier
            // ReSharper disable IdentifierTypo
            """;

        foreach (var (path, writer) in Writers)
        {
            using var sw = File.CreateText(Path.Combine(Directory, path));

            sw.WriteLine(header);

            sw.WriteLine(writer.InnerWriter.ToString());
        }
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