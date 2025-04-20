// ReSharper disable StringLiteralTypo
// ReSharper disable CommentTypo

using System.Collections;
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
                    Dependencies.Add(new SymKey(SymbolStorageClass.TPDEF, m.Type.Kind));
                }
                else
                {
                    SymbolStorageClass cClass;

                    if (m.Type.Kind == SymbolTypeKind.STRUCT)
                    {
                        cClass = SymbolStorageClass.STRTAG;
                    }
                    else if (m.Type.Kind == SymbolTypeKind.UNION)
                    {
                        cClass = SymbolStorageClass.UNTAG;
                    }
                    else
                    {
                        cClass = SymbolStorageClass.TPDEF;
                    }

                    // Dependencies.Add(new SymKey( m.Type.Kind, m2.Tag));
                    if (m2.Tag == def.Name)
                    {
                        //Console.WriteLine(def); // self referencing, e.g. _GsCOORDINATE
                    }
                    else
                    {
                        if (Symbols.Any(s => s.Code.Any(t => t.IsTypedef(u => u.Name == m2.Tag))))
                        {
                            // TODO when this is valid then the above could be reworked/simplified

                            Dependencies.Add(new SymKey(SymbolStorageClass.TPDEF, m2.Tag)); // e.g. AFFECT.C/EditorSave/Level
                        }
                        else
                        {
                            Dependencies.Add(new SymKey(cClass, m2.Tag));
                        }
                    }
                }
            }
            else
            {
                Dependencies.Add(new SymKey(SymbolStorageClass.TPDEF, m.Type.Kind));
            }
        }
    }

    private void ResolveExt(ISymbolDefinition def)
    {
        Name = new SymKey(def.Class, def.Name);

        Dependencies.Add(new SymKey(SymbolStorageClass.TPDEF, def.Type.Kind));
    }
}