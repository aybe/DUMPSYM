namespace DUMPSYM;

public sealed class SymbolRecordFunctionStart(SymbolContext context) : SymbolRecord, ISymbolFunction, ISymbolName
{
    public ushort FramePointer { get; } = context.Read<ushort>();

    public uint Size { get; } = context.Read<uint>();

    public ushort ReturnAddressRegister { get; } = context.Read<ushort>();

    public uint Mask { get; } = context.Read<uint>();

    public int MaskOffset { get; } = context.Read<int>();

    public uint Line { get; } = context.Read<uint>();

    public string File { get; } = context.ReadStringAscii();

    public string Name { get; } = context.ReadStringAscii();

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