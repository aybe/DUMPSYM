namespace DUMPSYM.Symbols;

public interface ISymbol
{
    public bool IsExternal => this is ISymbolDefinition { Class: SymbolStorageClass.EXT };

    public bool IsFileStart => this is ISymbolFileStart;

    public bool IsFileEnd => this is ISymbolFileEnd;

    public bool IsFunctionStart => this is ISymbolFunction;

    public bool IsFunctionEnd => this is ISymbolFunctionEnd;

    public bool IsStatic => this is ISymbolDefinition { Class: SymbolStorageClass.STAT };

    public bool IsTypedef => this is ISymbolDefinition { Class: SymbolStorageClass.TPDEF };

    public bool IsVariable => this is ISymbolVariable;

    #region Types

    public bool IsEnum => this is ISymbolDefinition { Class: SymbolStorageClass.ENTAG, Type.Kind: SymbolTypeKind.ENUM };

    public bool IsStruct => this is ISymbolDefinition { Class: SymbolStorageClass.STRTAG, Type.Kind: SymbolTypeKind.STRUCT };

    public bool IsUnion => this is ISymbolDefinition { Class: SymbolStorageClass.UNTAG, Type.Kind: SymbolTypeKind.UNION };

    public bool IsTypeEnd => this is ISymbolDefinition { Class: SymbolStorageClass.EOS, Type.Kind: SymbolTypeKind.NULL, Name: ".eos" };

    #endregion
}