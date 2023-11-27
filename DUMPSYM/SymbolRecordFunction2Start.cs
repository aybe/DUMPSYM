using Whatever.Extensions;

namespace DUMPSYM;

public sealed class SymbolRecordFunction2Start(Stream stream) : SymbolRecord
{
    public ushort FramePointer { get; } = stream.Read<ushort>();

    public uint Size { get; } = stream.Read<uint>();

    public ushort ReturnAddressRegister { get; } = stream.Read<ushort>();

    public uint Mask { get; } = stream.Read<uint>();

    public int MaskOffset { get; } = stream.Read<int>();

    public uint FMask { get; } = stream.Read<uint>();

    public int FMaskOffset { get; } = stream.Read<int>();

    public uint Line { get; } = stream.Read<uint>();

    public string Path { get; } = stream.ReadStringAscii(stream.Read<byte>());

    public string Name { get; } = stream.ReadStringAscii(stream.Read<byte>());

    public override void Write(SymbolHeader header, TextWriter writer, uint line)
    {
        header.WriteHeaderPositionAddressType(writer);

        writer.WriteLine("Function2 start");
        writer.WriteLine($"    fp = {FramePointer}");
        writer.WriteLine($"    fsize = {Size}");
        writer.WriteLine($"    retreg = {ReturnAddressRegister}");
        writer.WriteLine($"    mask = ${Mask:x8}");
        writer.WriteLine($"    maskoffs = {MaskOffset}");
        writer.WriteLine($"    fmask = ${FMask:x8}");
        writer.WriteLine($"    fmaskoffs = {FMaskOffset}");
        writer.WriteLine($"    line = {Line}");
        writer.WriteLine($"    file = {Path}");
        writer.WriteLine($"    name = {Name}");
    }
}