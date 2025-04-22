//#define LOG
// ReSharper disable CommentTypo

using System.Diagnostics;
using System.Runtime.CompilerServices;

namespace DUMPSYM.Tests;

public static class SymbolCleaner
{
    public static Symbol[][] PreProcessSymbols(Symbol[][] lists)
    {
        foreach (var list in lists)
        {
            RenameType(list);
            RenameTypeMembers(list);
            RenameTypeDef(list);
        }

        var changes = new List<Symbol[]>();

        var pass = 0;

        while (true)
        {
            foreach (var list in lists)
            {
                Parse(list, lists, changes);
            }

            if (changes.Count == 0)
            {
                break;
            }

            lists = lists.Where(s => !changes.Contains(s)).ToArray();

            changes.Clear();

            pass++;
        }

        Log($"Passes performed: {pass}");
        Log("EOF");
        Log("EOF");
        Log("EOF");

        return lists;
    }

    [Conditional("LOG")]
    private static void Log(object? value = null, [CallerMemberName] string memberName = null!)
    {
        Console.WriteLine($"{memberName}: {value}");
    }

    private static void Parse(Symbol[] list, Symbol[][] lists, List<Symbol[]> changes)
    {
        foreach (var symbol in list)
        {
            if (symbol.Record is not ISymbolDefinition definition)
            {
                break; // function, file, line
            }

            switch (definition.Class)
            {
                case SymbolStorageClass.EXT:
                    ParseExternal(list);
                    break;
                case SymbolStorageClass.TPDEF:

                    if (ParseTypeDef(list, lists) is { } s1)
                    {
                        changes.Add(s1);
                    }

                    break;
                case SymbolStorageClass.STRTAG:
                    if (ParseTypeStruct(list) is { } s2)
                    {
                        changes.Add(s2);
                    }

                    break;
                case SymbolStorageClass.UNTAG:
                    if (ParseTypeUnion(list) is { } s3)
                    {
                        changes.Add(s3);
                    }

                    break;
                default:
                    throw new InvalidOperationException(definition.ToString());
            }

            break;
        }
    }

    private static void ParseExternal(Symbol[] symbols)
    {
        // TODO
        Assert.AreEqual(1, symbols.Length);
    }

    private static Symbol[]? ParseTypeDef(Symbol[] current, Symbol[][] everything) // TODO bool
        // TODO maybe things like Def2 class TPDEF type PTR STRUCT size 4 dims 0 tag _physadr name physadr
        // TODO maybe things like Def2 class TPDEF type STRUCT size 48 dims 0 tag label_t name label_t
        // TODO maybe things like Def2 class TPDEF type STRUCT size 8 dims 0 tag _quad name quad
    {
        Assert.AreEqual(1, current.Length);

        var symbol1 = current[0];

        var changed = false;

        if (symbol1.Record is not ISymbolDefinition2 typedef)
        {
            return null;
        }

        if (!SymbolRegistry.HasFakeName(typedef.Tag))
        {
            return null;
        }

        // if there is a typedef with a fake tag
        //     rename the relevant struct with that fake name with typedef name
        //         rename the .eos item of the struct
        //     for each struct
        //         if struct has a member whose tag is that fake tag
        //             change the tag of the member to the typedef name
        //     delete the typedef
        Log($"OK: {typedef}");

        foreach (var symbols in everything)
        {
            if (symbols[0].Record is not ISymbolDefinition { Class: SymbolStorageClass.STRTAG or SymbolStorageClass.UNTAG } header)
            {
                continue;
            }

            if (header.Name == typedef.Tag)
            {
                Log($"\tRENAME STRUCT: {header}"); // TODO delete
                header.Name = typedef.Name;
                //Log($"\tRENAME STRUCT: {header}"); // TODO delete


                var footer = symbols[^1].Record as ISymbolDefinition2;

                Assert.IsNotNull(footer);
                Assert.AreEqual(SymbolStorageClass.EOS, footer.Class);
                Assert.AreEqual(typedef.Tag, footer.Tag);
                Assert.AreEqual(".eos", footer.Name);

                Log($"\tRENAME FOOTER: {footer}"); // TODO delete
                footer.Tag = typedef.Name;
                //Log($"\tRENAME FOOTER: {footer}"); // TODO delete


                changed = true;
            }

            var members = symbols[1..^1];

            foreach (var member in members)
            {
                if (member.Record is not ISymbolDefinition2 member2)
                {
                    continue;
                }

                if (member2.Tag != typedef.Tag)
                {
                    continue;
                }

                Log($"\t\tRENAME MEMBER: {member2}");
                member2.Tag = typedef.Name;
                //Log($"\t\tRENAME MEMBER: {member2}");
                changed = true;
            }
        }

        return changed ? current : null;
    }

    private static Symbol[]? ParseType(Symbol[] symbols)
    {
        // TODO
        Assert.AreNotEqual(1, symbols.Length);
        return null;
    }

    private static Symbol[]? ParseTypeUnion(Symbol[] symbols)
    {
        ParseType(symbols);
        return null;
    }

    private static Symbol[]? ParseTypeStruct(Symbol[] symbols)
    {
        ParseType(symbols);
        return null;
    }

    #region Rename

    private static void RenameType(Symbol[] symbols)
    {
        if (symbols[0].Record is ISymbolDefinition def)
        {
            if (def.Class is SymbolStorageClass.STRTAG or SymbolStorageClass.UNTAG)
            {
                if (PsxRuntimeLibrary.Names.TryGetValue(def.Name, out var value))
                {
                    Console.WriteLine($"OK: {def} -> {value}");

                    def.Name = value;
                }
            }
        }
    }

    private static void RenameTypeMembers(Symbol[] symbols)
    {
        if (symbols[0].Record is ISymbolDefinition def)
        {
            if (def.Class is SymbolStorageClass.STRTAG or SymbolStorageClass.UNTAG)
            {
                var members = symbols[1..^1];

                foreach (var member in members)
                {
                    if (member.Record is ISymbolDefinition2 member2)
                    {
                        if (PsxRuntimeLibrary.Names.TryGetValue(member2.Tag, out var value))
                        {
                            Console.WriteLine($"OK: {member2} -> {value}");

                            member2.Tag = value;
                        }
                    }
                }
            }
        }
    }

    private static void RenameTypeDef(Symbol[] symbols)
    {
        if (symbols[0].Record is ISymbolDefinition2 def)
        {
            if (def.Class is (SymbolStorageClass.TPDEF))
            {
                if (PsxRuntimeLibrary.Names.TryGetValue(def.Tag, out var value))
                {
                    Console.WriteLine($"OK: {def} -> {value}");

                    def.Tag = value;
                }
            }
        }
    }

    #endregion
}