namespace DUMPSYM;

[Serializable]
public sealed class SymbolRecordFunction2Start : SymbolRecord, ISymbolFunction, ISymbolName
{
    public SymbolRecordFunction2Start()
    {
    }

    public SymbolRecordFunction2Start(SymbolContext context)
    {
        FMask = context.Read<uint>();
        FMaskOffset = context.Read<int>();
        FramePointer = context.Read<ushort>();
        Size = context.Read<uint>();
        ReturnAddressRegister = context.Read<ushort>();
        Mask = context.Read<uint>();
        MaskOffset = context.Read<int>();
        Line = context.Read<uint>();
        File = context.ReadStringAscii();
        Name = context.ReadStringAscii();
    }

    public uint FMask { get; set; }

    public int FMaskOffset { get; set; }

    public ushort FramePointer { get; set; }

    public uint Size { get; set; }

    public ushort ReturnAddressRegister { get; set; }

    public uint Mask { get; set; }

    public int MaskOffset { get; set; }

    public uint Line { get; set; }

    public string File { get; set; } = null!;

    public string Name { get; set; } = null!;
}