namespace DUMPSYM;

public sealed class SymbolRecordFunction2Start : SymbolRecord, ISymbolFunction, ISymbolName
{
    public SymbolRecordFunction2Start(SymbolContext context) : base(context)
    {
        FramePointer = context.Read<ushort>();
        Size = context.Read<uint>();
        ReturnAddressRegister = context.Read<ushort>();
        Mask = context.Read<uint>();
        MaskOffset = context.Read<int>();
        FMask = context.Read<uint>();
        FMaskOffset = context.Read<int>();
        Line = context.Read<uint>();
        File = context.ReadStringAscii();
        Name = context.ReadStringAscii();
    }

    public uint FMask { get; }

    public int FMaskOffset { get; }

    public ushort FramePointer { get; }

    public uint Size { get; }

    public ushort ReturnAddressRegister { get; }

    public uint Mask { get; }

    public int MaskOffset { get; }

    public uint Line { get; }

    public string File { get; }

    public string Name { get; }
}