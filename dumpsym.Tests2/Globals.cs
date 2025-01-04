// ReSharper disable IdentifierTypo
// ReSharper disable InconsistentNaming
// ReSharper disable CommentTypo
// ReSharper disable StringLiteralTypo

namespace dumpsym.Tests2;

public static class Globals
{
    private static readonly byte[] class_types =
    {
        0x6B, 0x6A, 0x69, 0x68, 0x67, 0x66, 0x65, 0x13, 0x12,
        0x11, 0x10, 0x0F, 0x0E, 0x0D, 0x0C, 0x0B, 0x0A, 0x09,
        0x08, 0x07, 0x06, 0x05, 0x04, 0x03, 0x02, 0x01, 0x00
    };

    private static unsafe void parse_class(uint class_type1)
    {
        Console.Write("class ");

        if (class_type1 + 1 > 0x6B)
        {
            Console.Write($"?{class_type1}? ");
            return;
        }

        var index = 28;
        fixed (byte* tp = &class_types[0])
        {
            var b = tp;
            bool found;
            do
            {
                if (index == 0)
                {
                    break;
                }

                found = *b == (byte)(class_type1 + 1);
                b++;
                index--;
            } while (found == false);

            var type = index switch
            {
                0  => $"?{class_type1}?",
                1  => "EFCN",
                2  => "NULL",
                3  => "AUTO",
                4  => "EXT",
                5  => "STAT",
                6  => "REG",
                7  => "EXTDEF",
                8  => "LABEL",
                9  => "ULABEL",
                10 => "MOS",
                11 => "ARG",
                12 => "STRTAG",
                13 => "MOU",
                14 => "UNTAG",
                15 => "TPDEF",
                16 => "USTATIC",
                17 => "ENTAG",
                18 => "MOE",
                19 => "REGPARM",
                20 => "FIELD",
                21 => "BLOCK",
                22 => "FCN",
                23 => "EOS",
                24 => "FILE",
                25 => "LINE",
                26 => "ALIAS",
                27 => "HIDDEN",
                _  => throw new NotSupportedException(index.ToString())
            };

            Console.Write($"{type} ");
        }
    }

    private static void parse_type(int class_type2)
    {
        Console.Write("type ");

        while ((class_type2 & 0xFFF0) != 0)
        {
            var type1 = (uint)((class_type2 >> 4) & 3); // eax

            var type2 = type1 switch
            {
                0 => throw new NotSupportedException(type1.ToString()),
                1 => "PTR",
                2 => "FCN",
                _ => "ARY"
            };

            Console.Write($"{type2} ");

            class_type2 = ((class_type2 >> 2) & 0xFFF0) + (class_type2 & 0xF);
        }

        var type = class_type2 switch
        {
            0  => "NULL",
            1  => "VOID",
            2  => "CHAR",
            3  => "SHORT",
            4  => "INT",
            5  => "LONG",
            6  => "FLOAT",
            7  => "DOUBLE",
            8  => "STRUCT",
            9  => "UNION",
            10 => "ENUM",
            11 => "MOE",
            12 => "UCHAR",
            13 => "USHORT",
            14 => "UINT",
            15 => "ULONG",
            _  => throw new NotSupportedException(class_type2.ToString())
        };

        Console.Write($"{type} ");
    }

    private static string ReadString(Stream stream)
    {
        var count = stream.ReadByte();
        
        var ascii = stream.ReadStringAscii(count);

        return ascii;
    }

    internal static int __main(string[] args)
    {
        if (args.Length != 2)
        {
            Console.WriteLine("dumpsym 2.02 (c) 1997 SN Systems Software Ltd");
            Console.WriteLine("Usage: dumpsym sym_file");
            return 1;
        }

        using var f = File.OpenRead(args[1]);

        long line_num = 0;

        var sig = f.ReadStringAscii(3);

        if (sig != "MND")
        {
            Console.WriteLine($"Error: File '{args[1]}' is not in SN-SYM format");
            return 1;
        }

        Console.Write($"\nHeader : {sig}");
        var ver = f.ReadByte();
        Console.WriteLine($" version {ver}");
        var unit = f.ReadByte();
        Console.WriteLine($"Target unit {unit}");
        f.ReadByte();
        f.ReadByte();
        f.ReadByte();

        while (f.Position < f.Length)
        {
            int tag;
            uint offset;
            while (true)
            {
                var bin_pos = (int)f.Position;
                Console.Write($"{bin_pos:x6}: ");
                offset = f.ReadUInt32();
                tag = f.ReadByte();
                if (tag != 8)
                {
                    break;
                }

                var mx_info = f.ReadByte();
                Console.WriteLine($"${offset:x8} {8:x} MX-info {mx_info:x}");
            }

            if (tag == -1)
            {
                break;
            }

            if (tag <= 0x7F)
            {
                var identifier = ReadString(f);
                Console.WriteLine($"${offset:x8} {tag:x} {identifier}");
                continue;
            }

            switch (tag)
            {
                case 0x80:
                    Console.WriteLine($"${offset:x8} {0x80:x} Inc SLD linenum (to {++line_num})");
                    break;
                case 0x82:
                    var byte_add = f.ReadByte();
                    line_num += byte_add;
                    Console.Write($"${offset:x8} {tag:x} Inc SLD linenum by byte {byte_add} (to {line_num})\n");
                    break;
                case 0x84:
                    var word_add = f.ReadUInt16();
                    line_num += word_add;
                    Console.Write($"${offset:x8} {tag:x} Inc SLD linenum by word {word_add} (to {line_num})\n");
                    break;
                case 0x86:
                    line_num = f.ReadUInt32();
                    Console.WriteLine($"${offset:x8} {tag:x} Set SLD linenum to {line_num}");
                    break;
                case 0x88:
                    line_num = f.ReadUInt32();
                    Console.WriteLine($"${offset:x8} {tag:x} Set SLD to line {line_num} of file {ReadString(f)}");
                    break;
                case 0x8A:
                    Console.WriteLine($"${offset:x8} {tag:x} End SLD info");
                    break;
                case 0x8C:
                    Console.WriteLine($"${offset:x8} {0x8C:x} Function start");
                    var func_fp = f.ReadUInt16();
                    Console.WriteLine($"    fp = {func_fp}");
                    var func_fsize = f.ReadUInt32();
                    Console.WriteLine($"    fsize = {func_fsize}");
                    var func_retreg = f.ReadUInt16();
                    Console.WriteLine($"    retreg = {func_retreg}");
                    var func_mask = f.ReadUInt32();
                    Console.WriteLine($"    mask = ${func_mask:x8}");
                    var func_maskoffs = f.ReadUInt32();
                    Console.WriteLine($"    maskoffs = {(int)func_maskoffs}");
                    var func_line = f.ReadUInt32();
                    Console.WriteLine($"    line = {func_line}");
                    Console.Write("    file = " + ReadString(f));
                    Console.Write("\n    name = " + ReadString(f));
                    Console.WriteLine("");
                    break;
                case 0x8E:
                    var func_end_line = f.ReadUInt32();
                    Console.WriteLine($"${offset:x8} {tag:x} Function end   line {func_end_line}");
                    break;
                case 0x90:
                    var block_start_line = f.ReadUInt32();
                    Console.WriteLine($"${offset:x8} {tag:x} Block start  line = {block_start_line}");
                    break;
                case 0x92:
                    var block_end_line = f.ReadUInt32();
                    Console.WriteLine($"${offset:x8} {tag:x} Block end  line = {block_end_line}");
                    break;
                case 0x94:
                    Console.Write($"${offset:x8} {0x94:x} Def ");
                    var class_def_type1 = (int)f.ReadUInt16();
                    parse_class((uint)class_def_type1);
                    var class_def_type2 = (int)f.ReadUInt16();
                    parse_type(class_def_type2);
                    var class_def_obj_size = f.ReadUInt32();
                    Console.Write($"size {class_def_obj_size} ");
                    Console.Write($"name {ReadString(f)}");
                    Console.WriteLine("");
                    break;
                case 0x96:
                    Console.Write($"${offset:x8} {tag:x} Def2 ");
                    var class_def2_type1 = (int)f.ReadUInt16();
                    parse_class((uint)class_def2_type1);
                    var class_def2_type2 = (int)f.ReadUInt16();
                    parse_type(class_def2_type2);
                    var class_def2_obj_size = f.ReadUInt32();
                    Console.Write($"size {class_def2_obj_size} ");
                    var class_def2_dims_count = f.ReadUInt16();
                    Console.Write($"dims {class_def2_dims_count} ");
                    for (; class_def2_dims_count != 0; --class_def2_dims_count)
                    {
                        var class_def2_dim = f.ReadUInt32();
                        Console.Write($"{class_def2_dim} ");
                    }

                    Console.Write($"tag {ReadString(f)}");
                    Console.Write($" name {ReadString(f)}");
                    Console.WriteLine("");
                    break;
                case 0x98:
                    var overlay_length = f.ReadUInt32();
                    var overlay_id = f.ReadUInt32();
                    Console.WriteLine($"${offset:x8} overlay length ${overlay_length:x8} id ${overlay_id:x}");
                    break;
                case 0x9A:
                    Console.WriteLine($"${offset:x8} set overlay");
                    break;
                case 0x9C:
                    Console.WriteLine($"${offset:x8} {tag:x} Function2 start");
                    var func2_fp = f.ReadUInt16();
                    Console.WriteLine($"    fp = {func2_fp}");
                    var func2_fsize = f.ReadUInt32();
                    Console.WriteLine($"    fsize = {func2_fsize}");
                    var func2_retreg = f.ReadUInt16();
                    Console.WriteLine($"    retreg = {func2_retreg}");
                    var func2_mask = f.ReadUInt32();
                    Console.WriteLine($"    mask = ${func2_mask:x8}");
                    var func2_maskoffs = f.ReadUInt32();
                    Console.WriteLine($"    maskoffs = {func2_maskoffs}");
                    var func2_fmask = f.ReadUInt32();
                    Console.WriteLine($"    fmask = ${func2_fmask:x8}");
                    var func2_fmaskoffs = f.ReadUInt32();
                    Console.WriteLine($"    fmaskoffs = {func2_fmaskoffs}");
                    var func2_line = f.ReadUInt32();
                    Console.WriteLine($"    line = {func2_line}");
                    Console.Write($"    file = {ReadString(f)}");
                    Console.Write($"\n    name = {ReadString(f)}");
                    Console.WriteLine("");
                    break;
                case 0x9E:
                    Console.Write($"${offset:x8} {0x9E:x} Mangled name \"{ReadString(f)}");
                    Console.Write($"\" is \"{ReadString(f)}");
                    Console.WriteLine("");
                    break;
                default:
                    Console.WriteLine($"??? {tag} ???\n");
                    break;
            }
        }

        return 0;
    }
}