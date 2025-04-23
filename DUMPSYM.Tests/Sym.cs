#define DEPEND_ON_TYPEDEF_AND_TYPE // adds a dependency to the type in addition to a dependency to the typedef, e.g. AFFECT.C/EditorSave/Level
// ReSharper disable StringLiteralTypo
// ReSharper disable CommentTypo

using DUMPSYM.Extensions;

namespace DUMPSYM.Tests;

public class Sym
{
    public SymKey Name { get; set; }

    public HashSet<SymKey> Dependencies { get; init; } = [];

    public required Code Code { get; init; }

    public SymbolPriority? Priority { get; set; }

    private List<Sym> Symbols { get; set; }
    public SortingSettings Settings { get; set; }

    #region Priorities

    private static SymbolPriority PriorityTypeDefBasic { get; set; } = null!;

    private static SymbolPriority PriorityTypeFake { get; set; } = null!;

    private static SymbolPriority PriorityTypeGame { get; set; } = null!; // -100_000 // TODO adjust

    private static SymbolPriority PriorityTypeDefGame { get; set; } = null!;

    private static SymbolPriority PrioritySymExternal { get; set; } = null!;

    private static SymbolPriority PrioritySymFunction { get; set; } = null!;

    private static SymbolPriority PrioritySymFile { get; set; } = null!;

    private static SymbolPriority PrioritySymStatic { get; set; } = null!;

    private static SymbolPriority PrioritySymName { get; set; } = null!;

    private static SymbolPriority PrioritySymEndOfFile { get; set; } = null!;

    #endregion

    public override string ToString()
    {
        return $"{nameof(Name)}: {Name}, {nameof(Dependencies)}: [{string.Join(", ", Dependencies)}], {nameof(Priority)}: {Priority}";
    }

    public static void Initialize()
    {
        // TODO basic typdefs come from the SDK, update PsxRuntimeLibrary and label

        // TODO move priority for PsxRuntimeLibrary.StructuresPriority because it makes no sense

        PriorityTypeDefBasic = new SymbolPriority(-2_000_000, "TPDEF basic");

        PriorityTypeFake = new SymbolPriority(-1_000_000, "TYPE fake");

        PriorityTypeGame = new SymbolPriority(PsxRuntimeLibrary.StructuresPriority.Value / 2, "TYPE game");

        PriorityTypeDefGame = new SymbolPriority(PsxRuntimeLibrary.StructuresPriority.Value - 2000, "TPDEF game");

        PrioritySymExternal = new SymbolPriority(+1_000_000, "SYM EXT");

        PrioritySymFunction = new SymbolPriority(+2_000_000, "SYM FUNC");

        PrioritySymFile = new SymbolPriority(+3_000_000, "SYM FILE");

        PrioritySymStatic = new SymbolPriority(+5_000_000, "SYM STAT");

        PrioritySymName = new SymbolPriority(+6_000_000, "SYM NAME");

        PrioritySymEndOfFile = new SymbolPriority(+7_000_000, "SYM EOF");
    }

    public void ResolveDependencies(List<Sym> symbols)
    {
        Symbols = symbols;

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
                Priority = PrioritySymFile;
                break; // NONE
            case ISymbolFileEnd:
                Name = new SymKey("EOF");
                Priority = PrioritySymEndOfFile;
                break;
            case ISymbolFunction func:
                Name = new SymKey("FUNC", func.Name); // TODO
                Priority = PrioritySymFunction;
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

        Priority = PrioritySymStatic;

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

            Priority = PriorityTypeDefGame;
        }
        else
        {
            Priority = PriorityTypeDefBasic++; // e.g. Def class TPDEF type UCHAR size 0 name BBOOL
        }
    }

    private void ResolveType(ISymbolDefinition def)
    {
        Name = new SymKey(def.Class, def.Name);

        var hasFakeName = SymbolRegistry.HasFakeName(def.Name);

        Priority ??= new SymbolPriority(0, hasFakeName ? "Fake Type" : "Real Type"); // TODO looks wrong

        var members = Code[1..^1];
        
        foreach (var m in members.Cast<ISymbolDefinition>())
        {
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
                            Priority = new SymbolPriority(-1, "STRUCT MEMBER");
                        }
                    }
                }
            }
            else // typedef is a primitive
            {
                Dependencies.Add(new SymKey(SymbolStorageClass.TPDEF, m.Type.Kind));
            }
        }

        if (hasFakeName)
        {
            // push fake types up

            Priority--;

            // ADDED: push them up, sorted by name, assuming compiler did it right

            //var i = int.Parse(RegexFakeName.Match(def.Name).Groups[1].Value);
            //
            //const int extra = 1; // so they end up after non-existing SymbolTypeKind primitives (i.e. game-specific), e.g. Def class TPDEF type UCHAR size 0 name BBOOL
            //
            //Priority = PriorityTypedefBasicEnd + i + extra;

            Priority = PriorityTypeFake++;
        }
        else
        {
            if (PsxRuntimeLibrary.Structures.AsSpan().IndexOf(def.Name) is var i && i != -1)
            {
                Priority = PsxRuntimeLibrary.StructuresPriority with { Value = PsxRuntimeLibrary.StructuresPriority.Value + i };
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

                    Assert.AreEqual(0, Priority.Value);

                    if (hasFakeName)
                    {
                        Assert.Fail();
                    }
                    else
                    {
                        Priority = PriorityTypeGame; // TODO its typedef depends on PSX structures priority

                        if (Symbols.Any(s => s.Code.Any(t => t.IsTypedef(u => u is ISymbolDefinition2 sd2 && sd2.Tag == def.Name))))
                        {
                            Priority++; // when it's a game type that depends on other game types, push it down
                        }
                    }
                }
            }
        }
    }

    private void ResolveVariable(ISymbolVariable variable) // TODO rework this crap, could be other than STAT
    {
        Name = new SymKey("NAME", variable.Name);

        Priority = PrioritySymName;

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

        Priority = PrioritySymExternal;

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