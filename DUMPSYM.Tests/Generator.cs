using System.CodeDom.Compiler;
using System.Collections.Frozen;
using System.Collections.Immutable;
using System.Diagnostics.CodeAnalysis;
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

    private GeneratorCounters Counters { get; } = new();

    private GeneratorOptions Options { get; }

    private GeneratorSets Sets { get; } = new();

    private Dictionary<string, IndentedTextWriter> Writers { get; } = new();

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
            else
            {
                continue;
                throw new NotImplementedException(header.ToString());
            }
        }
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
            writer.WriteLine($"{SymbolGenerator.ToString(type.Class!.Value)} {type.Name}; // {type}");

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

        if (name is "clock_t" or "size_t" or "wchar_t")
        {
            writer.WriteLine(GetIgnored(header));

            counter.Ignored++;

            return;
        }

        if (Sets.Typedefs.Add(header))
        {
            writer.WriteLine($"typedef int {name}; // {header}");

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
            if (!TryGetHeader(symbol, out path!))
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

    private bool TryGetHeader(Symbol type, [MaybeNullWhen(false)] out string result) // TODO improve
    {
        result = null;

        if (!TryGetDefinition(type, out var def))
        {
            return false;
        }

        if (!SdkMatch.DefinitionsMap.TryGetValue(def.Name!, out result))
        {
            return false;
        }

        return true;
    }

    public void Write()
    {
        const string header = // TODO external
            """
            #pragma once
            // ReSharper disable CommentTypo
            // ReSharper disable CppInconsistentNaming
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