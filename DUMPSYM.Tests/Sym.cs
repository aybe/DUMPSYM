#define DEPEND_ON_TYPEDEF_AND_TYPE // adds a dependency to the type in addition to a dependency to the typedef, e.g. AFFECT.C/EditorSave/Level
#define SORT_TYPEDEFS_OF_FAKES // whether to sort typedefs that represents fake types by name // BUG has duplicate keys somewhere
// ReSharper disable StringLiteralTypo
// ReSharper disable CommentTypo

using System.Diagnostics;
using System.Text.RegularExpressions;
using DUMPSYM.Extensions;

namespace DUMPSYM.Tests;

public class Sym
{
    public SymKey Name { get; set; }

    public HashSet<SymKey> Dependencies { get; init; } = [];

    public required Code Code { get; init; }

    public int Priority { get; set; }

    private List<Sym> Symbols { get; set; }

    private static Regex RegexFakeName { get; } = new(@"^\.(\d+)fake$", RegexOptions.Compiled);

    private static int PriorityExternal { get; } = +1_000_000;

    private static int PriorityFunction { get; } = +2_000_000;

    private static int PriorityFile { get; } = +3_000_000;

    private int PriorityTypedefBasicStart { get; set; }

    private int PriorityTypedefBasicEnd { get; set; }

    public SortingSettings Settings { get; set; }

    private static Dictionary<string, int> PrioritiesTypedefFake { get; set; } = new(); // TODO this must be static atm otherwise it runs for each symbol

    private static int PriorityTypedefFakeStart { get; } = -1_000_000; // TODO adjust

    private static int PriorityGameType { get; } = -5; // TODO adjust

    private static int PriorityGameTypeDef => PsxRuntimeLibrary.StructuresPriority - 2000; // TODO adjust

    private int PriorityWithFilePosition => (int)(Settings.SortByFilePosition ? Code.Position : 0);

    public override string ToString()
    {
        return $"{nameof(Name)}: {Name}, {nameof(Dependencies)}: [{string.Join(", ", Dependencies)}], {nameof(Priority)}: {Priority}";
    }

    private void Initialize()
    {
        var kinds = Enum.GetValues<SymbolTypeKind>();

        var max = Convert.ToInt32(kinds.Max());

        PriorityTypedefBasicStart = int.MinValue;

        PriorityTypedefBasicEnd = PriorityTypedefBasicStart + max + 1;

        InitializeTypedefsFakes();
    }

    [Conditional("SORT_TYPEDEFS_OF_FAKES")]
    private void InitializeTypedefsFakes()
    {
        // this generates priorities for typedefs of fakes so they can be sorted by name

        if (PrioritiesTypedefFake.Count != 0)
        {
            return;
        }

        var names = new HashSet<string>();

        foreach (var sym in Symbols)
        {
            foreach (var symbol in sym.Code)
            {
                if (symbol is ISymbolDefinition2 { Class: SymbolStorageClass.TPDEF } def2 && SymbolRegistry.HasFakeName(def2.Tag))
                {
                    names.Add(def2.Name);
                }

                break;
            }
        }

        PrioritiesTypedefFake = names.OrderBy(s => s).Select((s, t) => (s, t)).ToDictionary(s => s.s, s => s.t);
    }

    public void ResolveDependencies(List<Sym> symbols)
    {
        Symbols = symbols;

        Initialize();

        var symbol = Code[0];

        switch (symbol)
        {
            case ISymbolDefinition { Class: SymbolStorageClass.EXT } def:
                ResolveExt(def);
                break;
            case ISymbolDefinition { Class: SymbolStorageClass.STAT } def:
                ResolveStatic(def);
                break;
            case ISymbolDefinition { Class: SymbolStorageClass.STRTAG } def:
                ResolveType(def);
                break;
            case ISymbolDefinition { Class: SymbolStorageClass.UNTAG } def:
                ResolveType(def);
                break;
            case ISymbolDefinition { Class: SymbolStorageClass.TPDEF } def:
                ResolveTypedef(def);
                break;
            case ISymbolFileStart file:
                Name = new SymKey("FILE", ((SymbolRecordSetSldToLineOfFile)file).File); // TODO can't use SymbolStorageClass.FILE here?
                Priority = PriorityFile;
                break; // NONE
            case ISymbolFileEnd:
                Name = new SymKey("EOF");
                break;
            case ISymbolFunction func:
                Name = new SymKey("FUNC", func.Name); // TODO
                Priority = PriorityFunction;
                break; // NONE
            case ISymbolVariable variable:
                ResolveVariable(variable);
                break;
            default:
                throw new NotImplementedException(symbol.ToString());
        }
    }

    private void ResolveStatic(ISymbolDefinition def)
    {
        Name = new SymKey(def.Class, def.Name);

        if (def is not ISymbolDefinition2 def2)
        {
            Dependencies.Add(new SymKey(SymbolStorageClass.TPDEF, def.Type.Kind.ToString()));
            return;
        }

        if (string.IsNullOrWhiteSpace(def2.Tag))
        {
            Assert.IsTrue(def2.Type.Modifiers.Contains(SymbolTypeModifier.ARY));
            Dependencies.Add(new SymKey(SymbolStorageClass.TPDEF, def2.Type.Kind.ToString()));
            return;
        }

        var kind = def2.Type.Kind;

        var ssc = kind switch
        {
            SymbolTypeKind.STRUCT => SymbolStorageClass.STRTAG,
            _                     => throw new NotImplementedException(kind.ToString())
        };

        Dependencies.Add(new SymKey(ssc, def2.Tag));
    }

    private void ResolveTypedef(ISymbolDefinition def)
    {
        Name = new SymKey(def.Class, def.Name);

        if (def is ISymbolDefinition2 t2)
        {
            Assert.IsFalse(string.IsNullOrWhiteSpace(t2.Tag));
            // is an alias to another typedef
            // Def2 class TPDEF type PTR STRUCT size 44 dims 0 tag LoadFiles name LoadFilesPtr

            SymbolStorageClass cClass;

            if (def.Type.Kind == SymbolTypeKind.STRUCT)
            {
                cClass = SymbolStorageClass.STRTAG;
            }
            else if (def.Type.Kind == SymbolTypeKind.UNION)
            {
                cClass = SymbolStorageClass.UNTAG;
            }
            else
            {
                cClass = SymbolStorageClass.TPDEF;
            }

            Dependencies.Add(new SymKey(cClass, t2.Tag)); // BUG this prevents sorting many symbols

            Priority = PriorityGameTypeDef;

            if (SymbolRegistry.HasFakeName(t2.Tag))
            {
                // this will move all typedefs that rely on fakes up the list and group them together which is great
                Priority = PriorityTypedefFakeStart;
#if SORT_TYPEDEFS_OF_FAKES
                var key = t2.Name;

                if (PrioritiesTypedefFake.TryGetValue(key, out var value))
                {
                    Priority += value;
                }
                else
                {
                    Console.WriteLine($"typedef has no symbol and won't be sorted: {key}"); // TODO decide what to do in this case
                }
#endif
            }
        }
        else
        {
            if (def.Name == def.Type.Kind.ToString())
            {
                // no dependency, e.g. Def class TPDEF type ULONG size 0 name ULONG
                Priority = PriorityTypedefBasicStart + Convert.ToInt32(def.Type.Kind);
            }
            else
            {
                // is an alias to another typedef
                //Console.WriteLine(def);
                var key = new SymKey(SymbolStorageClass.TPDEF, def.Type.Kind);

                if (Name == key)
                {
                    Dependencies.Add(key);
                    //Console.WriteLine(this);
                }

                Priority = PriorityTypedefBasicEnd; // e.g. Def class TPDEF type UCHAR size 0 name BBOOL // TODO delete

                // this is a new value that uses file position as well // TODO others should be sorted like so

                Priority = PriorityTypedefBasicEnd + PriorityWithFilePosition; // e.g. Def class TPDEF type UCHAR size 0 name BBOOL
            }
        }
    }

    private void ResolveType(ISymbolDefinition def)
    {
        Name = new SymKey(def.Class, def.Name);
        // return; // TODO adding a return here makes the test pass, bug below
        var members = Code[1..^1];

        if (def.Name == "EditorSave")
        {
            var w = 0;
        }

        foreach (var m in members.Cast<ISymbolDefinition>())
        {
            if (def.Name == "EditorSave" && m.Name == "Level")
            {
                var w = 0;
            }

            if (m is ISymbolDefinition2 m2)
            {
                if (string.IsNullOrWhiteSpace(m2.Tag))
                {
                    Assert.IsTrue(m2.Type.Modifiers.Contains(SymbolTypeModifier.ARY));
                    Dependencies.Add(new SymKey(SymbolStorageClass.TPDEF, m.Type.Kind)); // simple type as array
                }
                else
                {
                    if (m2.Tag == def.Name) // self referencing, e.g. _GsCOORDINATE
                    {
                        // NOP
                    }
                    else
                    {
                        // ReSharper disable once SwitchExpressionHandlesSomeKnownEnumValuesWithExceptionInDefault
                        var ssc = m.Type.Kind switch
                        {
                            SymbolTypeKind.STRUCT => SymbolStorageClass.STRTAG,
                            SymbolTypeKind.UNION  => SymbolStorageClass.UNTAG,
                            _                     => throw new InvalidDataException("Expected struct or union.")
                        };

                        if (Symbols.Any(s => s.Code.Any(t => t.IsTypedef(u => u.Name == m2.Tag))))
                        {
#if DEPEND_ON_TYPEDEF_AND_TYPE // BUG this doesn't change the order, whether done before or after
                            Dependencies.Add(new SymKey(ssc, m2.Tag));
#endif
                            // TODO when this is valid then the above could be reworked/simplified

                            Dependencies.Add(new SymKey(SymbolStorageClass.TPDEF, m2.Tag)); // e.g. AFFECT.C/EditorSave/Level
                        }
                        else // typedef is a primitive
                        {
                            Dependencies.Add(new SymKey(ssc, m2.Tag));
                            // this pushes the type further up the list, closer to its dependencies
                            // it results in the unrelated nodes between them to move somewhere else
                            // i.e. it's much more tight now
                            Priority = -1;
                        }
                    }
                }
            }
            else // typedef is a primitive
            {
                Dependencies.Add(new SymKey(SymbolStorageClass.TPDEF, m.Type.Kind));
            }
        }


        if (SymbolRegistry.HasFakeName(def.Name))
        {
            // push fake types up

            Priority--;

            // ADDED: push them up, sorted by name, assuming compiler did it right

            var i = int.Parse(RegexFakeName.Match(def.Name).Groups[1].Value);

            const int extra = 1; // so they end up after non-existing SymbolTypeKind primitives (i.e. game-specific), e.g. Def class TPDEF type UCHAR size 0 name BBOOL

            Priority = PriorityTypedefBasicEnd + i + extra;
        }
        else
        {
            if (PsxRuntimeLibrary.Structures.AsSpan().IndexOf(def.Name) is var i && i != -1)
            {
                Priority = PsxRuntimeLibrary.StructuresPriority + i;
            }
            else
            {
                if (members.Any(s => s is ISymbolDefinition2 def2 && !string.IsNullOrEmpty(def2.Tag)))
                {
                    // type has dependencies to other structs, leave it off like that

                    var zero = 0; // TODO
                }
                else // TODO this a trivial priority adjustment
                {
                    // type only depends on primitives, it can go further up the list

                    Assert.AreEqual(0, Priority);

                    if (SymbolRegistry.HasFakeName(def.Name))
                    {
                        Assert.Fail();
                    }
                    else
                    {
                        Priority = PriorityGameType; // TODO its typedef depends on PSX structures priority
                    }
                }
            }
        }
    }

    private void ResolveVariable(ISymbolVariable variable) // TODO rework this crap, could be other than STAT
    {
        Name = new SymKey("NAME", variable.Name);

        foreach (var sym in Symbols)
        {
            foreach (var symbol in sym.Code)
            {
                if (symbol is ISymbolDefinition { Class: SymbolStorageClass.STAT } def && def.Name == variable.Name)
                {
                    Dependencies.Add(new SymKey(SymbolStorageClass.STAT, variable.Name));
                    return;
                }
            }
        }

        // BUG MEMCARD.C/pad_status (and few others) has no corresponding definition, assuming integer for now...

        Dependencies.Add(new SymKey(SymbolStorageClass.TPDEF, SymbolTypeKind.LONG));
    }

    private void ResolveExt(ISymbolDefinition def)
    {
        Name = new SymKey(def.Class, def.Name);

        Priority = PriorityExternal;

        if (def is not ISymbolDefinition2 def2)
        {
            Dependencies.Add(new SymKey(SymbolStorageClass.TPDEF, def.Type.Kind));
            return;
        }

        var tag = def2.Tag;

        if (string.IsNullOrWhiteSpace(tag))
        {
            Assert.IsTrue(def2.Type.Modifiers.Contains(SymbolTypeModifier.ARY));
            Dependencies.Add(new SymKey(SymbolStorageClass.TPDEF, def2.Type.Kind));
            return;
        }

        // 1st: typedef (of type), 2nd: type

        if (Symbols.Any(s => s.Code.Any(t => t.IsTypedef(u => u.Name == tag))))
        {
            Dependencies.Add(new SymKey(SymbolStorageClass.TPDEF, tag));
            return;
        }

        var result = default(ISymbolDefinition); // TODO this syntax sucks

        // ReSharper disable once InvertIf
        if (Symbols.Any(s => s.Code.Any(t => t.IsType(u => u.Name == tag, out result)))) // TODO this syntax sucks
        {
            Dependencies.Add(new SymKey(result!.Class, result.Name));
            return;
        }

        throw new InvalidDataException(def2.ToString());
    }
}