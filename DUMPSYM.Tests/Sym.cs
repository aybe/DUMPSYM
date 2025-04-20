// ReSharper disable StringLiteralTypo
// ReSharper disable CommentTypo

using DUMPSYM.Extensions;

namespace DUMPSYM.Tests;

public class Sym
{
    public SymKey Name { get; set; }

    public HashSet<SymKey> Dependencies { get; init; } = [];

    public required Code Code { get; init; }

    private List<Sym> Symbols { get; set; }

    public override string ToString()
    {
        return $"{nameof(Name)}: {Name}, {nameof(Dependencies)}: [{string.Join(", ", Dependencies)}]";
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
                Name = new SymKey("FILE", ((SymbolRecordSetSldToLineOfFile)file).File); // TODO
                break; // NONE
            case ISymbolFunction func:
                Name = new SymKey("FUNC", func.Name); // TODO
                break; // NONE
            default:
                throw new NotImplementedException(symbol.ToString());
        }
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
        }
        else
        {
            if (def.Name == def.Type.Kind.ToString())
            {
                // no dependency
                // Def class TPDEF type ULONG size 0 name ULONG
                //Console.WriteLine(def);
                _ = 0;
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
                        if (Symbols.Any(s => s.Code.Any(t => t.IsTypedef(u => u.Name == m2.Tag))))
                        {
                            // TODO when this is valid then the above could be reworked/simplified

                            Dependencies.Add(new SymKey(SymbolStorageClass.TPDEF, m2.Tag)); // e.g. AFFECT.C/EditorSave/Level
                        }
                        else // typedef is a primitive
                        {
                            // ReSharper disable once SwitchExpressionHandlesSomeKnownEnumValuesWithExceptionInDefault
                            var ssc = m.Type.Kind switch
                            {
                                SymbolTypeKind.STRUCT => SymbolStorageClass.STRTAG,
                                SymbolTypeKind.UNION  => SymbolStorageClass.UNTAG,
                                _                     => throw new InvalidDataException("Expected struct or union.")
                            };
                            Dependencies.Add(new SymKey(ssc, m2.Tag));
                        }
                    }
                }
            }
            else // typedef is a primitive
            {
                Dependencies.Add(new SymKey(SymbolStorageClass.TPDEF, m.Type.Kind));
            }
        }
    }

    private void ResolveExt(ISymbolDefinition def)
    {
        Name = new SymKey(def.Class, def.Name);

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