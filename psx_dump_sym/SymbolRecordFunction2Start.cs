using Whatever.Extensions;

namespace psx_dump_sym;

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

    public string Path { get; } = SymbolUtility.ReadStringAscii(stream);

    public string Name { get; } = SymbolUtility.ReadStringAscii(stream);
}