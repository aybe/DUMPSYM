namespace DUMPSYM.Symbols;

public sealed record SymbolRecordFunctionStart : SymbolRecord, ISymbolFunction
{
    public SymbolRecordFunctionStart(SymbolContext context)
    {
        FramePointer = context.Read<ushort>();
        Size = context.Read<uint>();
        ReturnAddressRegister = context.Read<ushort>();
        Mask = context.Read<uint>();
        MaskOffset = context.Read<int>();
        Line = context.Read<uint>();
        File = context.ReadStringAscii();
        Name = context.ReadStringAscii();
    }

    public ushort FramePointer { get; set; }

    public uint Size { get; set; }

    public ushort ReturnAddressRegister { get; set; }

    public uint Mask { get; set; }

    public int MaskOffset { get; set; }

    public uint Line { get; set; }

    public string File { get; set; } = null!;

    public string Name { get; set; } = null!;

    public override string ToString()
    {
        return $"Function_start\r\n" +
               $"    fp = {FramePointer}\r\n" +
               $"    fsize = {Size}\r\n" +
               $"    retreg = {ReturnAddressRegister}\r\n" +
               $"    mask = ${Mask:x8}\r\n" +
               $"    maskoffs = {MaskOffset}\r\n" +
               $"    line = {Line}\r\n" +
               $"    file = {File}\r\n" +
               $"    name = {Name}";
    }
}