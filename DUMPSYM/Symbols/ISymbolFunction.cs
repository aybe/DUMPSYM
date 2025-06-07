namespace DUMPSYM.Symbols;

public interface ISymbolFunction : ISymbol
{
    ushort FramePointer { get; }

    uint Size { get; }

    ushort ReturnAddressRegister { get; }

    uint Mask { get; }

    int MaskOffset { get; }

    uint Line { get; }

    string File { get; }

    string Name { get; }
}