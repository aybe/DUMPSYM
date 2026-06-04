namespace DUMPSYM.Symbols;

public sealed record SymbolRecordFunction2Start : SymbolRecord, ISymbolFunction
{
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

    public override string ToString()
    {
        // BUG: should be DUMPSYM style
        return
            $"{base.ToString()}, " +
            $"{nameof(FMask)}: {FMask}, " +
            $"{nameof(FMaskOffset)}: {FMaskOffset}, " +
            $"{nameof(FramePointer)}: {FramePointer}, " +
            $"{nameof(Size)}: {Size}, " +
            $"{nameof(ReturnAddressRegister)}: {ReturnAddressRegister}, " +
            $"{nameof(Mask)}: {Mask}, " +
            $"{nameof(MaskOffset)}: {MaskOffset}, " +
            $"{nameof(Line)}: {Line}, " +
            $"{nameof(File)}: {File}, " +
            $"{nameof(Name)}: {Name}";
    }
}