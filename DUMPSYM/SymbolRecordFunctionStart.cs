namespace DUMPSYM;

public sealed class SymbolRecordFunctionStart : SymbolRecord, ISymbolFunction, ISymbolName
{
    public SymbolRecordFunctionStart(SymbolContext context) : base(context)
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