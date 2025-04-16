using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace DUMPSYM.Extensions;

public static class SymbolExtensions // TODO move
{
    #region Single

    private static List<ISymbol> Where(List<ISymbol> symbols, Func<ISymbol, bool> predicate)
    {
        return symbols.Where(predicate).ToList();
    }

    public static List<ISymbol> GetExternals(List<ISymbol> symbols)
    {
        return Where(symbols, s => s is ISymbolDefinition { Class: SymbolStorageClass.EXT });
    }

    public static List<ISymbol> GetFilesOrphans(List<ISymbol> symbols)
    {
        return Where(symbols, s => s is ISymbolFileEnd);
    }

    public static List<ISymbol> GetStatics(List<ISymbol> symbols)
    {
        return Where(symbols, s => s is ISymbolDefinition { Class: SymbolStorageClass.STAT });
    }

    public static List<ISymbol> GetTypedefs(List<ISymbol> symbols)
    {
        return Where(symbols, s => s is ISymbolDefinition { Class: SymbolStorageClass.TPDEF });
    }

    public static List<ISymbol> GetVariables(List<ISymbol> symbols)
    {
        return Where(symbols, s => s is ISymbolVariable);
    }

    #endregion

    #region Multiple

    private static List<List<ISymbol>> GetSymbols(List<ISymbol> symbols, Predicate<ISymbol> header, Predicate<ISymbol> footer)
    {
        var lists = new List<List<ISymbol>>();

        var index = 0;

        while (true)
        {
            var headerIndex = symbols.FindIndex(index, header);

            if (headerIndex == -1)
            {
                break;
            }

            index = headerIndex;

            var footerIndex = symbols.FindIndex(index, footer);

            index = footerIndex + 1;

            var list = symbols[headerIndex..(footerIndex + 1)];

            lists.Add(list);
        }

        return lists;
    }

    public static List<List<ISymbol>> GetFiles(List<ISymbol> symbols)
    {
        return GetSymbols(symbols, s => s.IsFileHeader(), s => s.IsFileFooter());
    }

    public static List<List<ISymbol>> GetFunctions(List<ISymbol> symbols)
    {
        return GetSymbols(symbols, s => s.IsFunctionHeader(), s => s.IsFunctionFooter());
    }

    public static List<List<ISymbol>> GetStructs(List<ISymbol> symbols)
    {
        return GetSymbols(symbols, s => s.IsStructHeader(), s => s.IsTypeFooter());
    }

    public static List<List<ISymbol>> GetUnions(List<ISymbol> symbols)
    {
        return GetSymbols(symbols, s => s.IsUnionHeader(), s => s.IsTypeFooter());
    }

    public static bool IsFileHeader(this ISymbol symbol)
    {
        return symbol is ISymbolFileStart;
    }

    public static bool IsFileFooter(this ISymbol symbol)
    {
        return symbol is ISymbolFileEnd;
    }

    public static bool IsFunctionHeader(this ISymbol symbol)
    {
        return symbol is ISymbolFunction;
    }

    public static bool IsFunctionFooter(this ISymbol symbol)
    {
        return symbol is ISymbolFunctionEnd;
    }

    public static bool IsStructHeader(this ISymbol symbol)
    {
        return symbol is ISymbolDefinition { Class: SymbolStorageClass.STRTAG, Type.Kind: SymbolTypeKind.STRUCT };
    }

    public static bool IsUnionHeader(this ISymbol symbol)
    {
        return symbol is ISymbolDefinition { Class: SymbolStorageClass.UNTAG, Type.Kind: SymbolTypeKind.UNION };
    }

    public static bool IsTypeFooter(this ISymbol symbol)
    {
        return symbol is ISymbolDefinition { Class: SymbolStorageClass.EOS, Type.Kind: SymbolTypeKind.NULL, Name: ".eos" };
    }

    #endregion

    #region Obsolete // TODO delete

    [Obsolete("Use other functions.")]
    public static List<LinkedList<SymbolRecord>> GetTypes(LinkedList<SymbolRecord> records)
    {
        var types = new List<LinkedList<SymbolRecord>>();

        var current = records.First;

        while (current != null)
        {
            if (!TryFindNode(IsHeader, current, out current, out var header))
            {
                continue;
            }

            var headerNode = current!;

            if (!TryFindNode(IsFooter, current, out current, out var footer))
            {
                continue;
            }

            var footerNode = current!;

            Assert.AreEqual(header!.Name, footer!.Tag);

            var type = new LinkedList<SymbolRecord>();

            type.CopyFrom(headerNode, footerNode);

            types.Add(type);
        }

        return types;

        static ISymbolDefinition? IsHeader(SymbolRecord record)
        {
            return record is ISymbolDefinition { Class: SymbolStorageClass.STRTAG, Type.Kind: SymbolTypeKind.STRUCT } def ? def : null;
        }

        static ISymbolDefinition2? IsFooter(SymbolRecord record)
        {
            return record is ISymbolDefinition2 { Class: SymbolStorageClass.EOS, Type.Kind: SymbolTypeKind.NULL, Name: ".eos" } def ? def : null;
        }
    }

    private static bool TryFindNode<TNode, TResult>(
        Selector<TNode, TResult> selector, LinkedListNode<TNode>? from, out LinkedListNode<TNode>? next, out TResult? result)
    {
        next = default;

        result = default;

        for (var node = from; node != null; node = node.Next)
        {
            result = selector(node.Value);

            if (result == null)
            {
                continue;
            }

            next = node;

            return true;
        }

        return false;
    }

    private delegate T? Selector<in TNode, out T>(TNode node);

    #endregion
}