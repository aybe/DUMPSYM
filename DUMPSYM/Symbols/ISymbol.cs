namespace DUMPSYM.Symbols;

public interface ISymbol
{
    public bool IsExternal => this is ISymbolDefinition { Class: SymbolStorageClass.EXT };

    public bool IsFileHeader => this is ISymbolFileStart;

    public bool IsFileFooter => this is ISymbolFileEnd;

    public bool IsFunctionHeader => this is ISymbolFunction;

    public bool IsFunctionFooter => this is ISymbolFunctionEnd;

    public bool IsStatic => this is ISymbolDefinition { Class: SymbolStorageClass.STAT };

    public bool IsTypedef => this is ISymbolDefinition { Class: SymbolStorageClass.TPDEF };

    public bool IsVariable => this is ISymbolVariable;

    #region Types

    public bool IsEnumHeader => this is ISymbolDefinition { Class: SymbolStorageClass.ENTAG, Type.Kind: SymbolTypeKind.ENUM };

    public bool IsStructHeader => this is ISymbolDefinition { Class: SymbolStorageClass.STRTAG, Type.Kind: SymbolTypeKind.STRUCT };

    public bool IsUnionHeader => this is ISymbolDefinition { Class: SymbolStorageClass.UNTAG, Type.Kind: SymbolTypeKind.UNION };

    public bool IsTypeFooter => this is ISymbolDefinition { Class: SymbolStorageClass.EOS, Type.Kind: SymbolTypeKind.NULL, Name: ".eos" };

    #endregion
}