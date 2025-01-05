namespace DUMPSYM;

public sealed class SymbolRecordFunction2Start(SymbolContext context) : SymbolRecord, ISymbolFunction, ISymbolName
{
    public uint FMask { get; } = context.Read<uint>();

    public int FMaskOffset { get; } = context.Read<int>();

    public ushort FramePointer { get; } = context.Read<ushort>();

    public uint Size { get; } = context.Read<uint>();

    public ushort ReturnAddressRegister { get; } = context.Read<ushort>();

    public uint Mask { get; } = context.Read<uint>();

    public int MaskOffset { get; } = context.Read<int>();

    public uint Line { get; } = context.Read<uint>();

    public string File { get; } = context.ReadStringAscii();

    public string Name { get; } = context.ReadStringAscii();
}