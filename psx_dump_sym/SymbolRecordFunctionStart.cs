using Whatever.Extensions;

namespace psx_dump_sym;

public sealed class SymbolRecordFunctionStart(Stream stream) : SymbolRecord
{
    public ushort FramePointer { get; } = stream.Read<ushort>();

    public uint Size { get; } = stream.Read<uint>();

    public ushort ReturnAddressRegister { get; } = stream.Read<ushort>();

    public uint Mask { get; } = stream.Read<uint>();

    public int MaskOffset { get; } = stream.Read<int>();

    public uint Line { get; } = stream.Read<uint>();

    public string Path { get; } = SymbolUtility.ReadStringAscii(stream);

    public string Name { get; } = SymbolUtility.ReadStringAscii(stream);

    public override void Write(SymbolHeader header, TextWriter writer, uint line)
    {
        header.WriteHeaderPositionAddressType(writer);

        writer.WriteLine("Function start");
        writer.WriteLine($"    fp = {FramePointer}");
        writer.WriteLine($"    fsize = {Size}");
        writer.WriteLine($"    retreg = {ReturnAddressRegister}");
        writer.WriteLine($"    mask = ${Mask:x8}");
        writer.WriteLine($"    maskoffs = {MaskOffset}");
        writer.WriteLine($"    line = {Line}");
        writer.WriteLine($"    file = {Path}");
        writer.WriteLine($"    name = {Name}");
    }
}