using System.IO.Compression;
using System.Text;

namespace kittywork.Wc3ReplayParser.Business;

public class ReplayParser : IReplayParser
{
    private const string Magic = "Warcraft III recorded game\u001A\0";

    public ReplayInfo Parse(string filePath)
    {
        using var fs = File.OpenRead(filePath);
        return Parse(fs);
    }

    public ReplayInfo Parse(Stream stream)
    {
        using var reader = new BinaryReader(stream, Encoding.ASCII, leaveOpen: true);
        var magicBytes = reader.ReadBytes(28);
        var magic = Encoding.ASCII.GetString(magicBytes);
        if (magic != Magic)
        {
            throw new InvalidDataException("Not a Warcraft III replay file.");
        }

        uint headerSize = reader.ReadUInt32();
        uint compressedSize = reader.ReadUInt32();
        uint headerVersion = reader.ReadUInt32();
        uint decompressedSize = reader.ReadUInt32();
        uint blockCount = reader.ReadUInt32();

        string gameId = string.Empty;
        uint version = 0;
        ushort build = 0;
        uint length = 0;

        if (headerVersion == 1)
        {
            gameId = Encoding.ASCII.GetString(reader.ReadBytes(4));
            version = reader.ReadUInt32();
            build = reader.ReadUInt16();
            reader.ReadUInt16(); // flags
            length = reader.ReadUInt32();
            reader.ReadUInt32(); // crc
        }

        stream.Seek(headerSize, SeekOrigin.Begin);
        var decompressed = new MemoryStream();
        for (int i = 0; i < blockCount; i++)
        {
            // W3Champions replay format uses 32-bit lengths
            uint compLen = reader.ReadUInt32();
            uint decompLen = reader.ReadUInt32();
            reader.ReadUInt32(); // checksum
            var compBytes = reader.ReadBytes((int)compLen);
            using var ms = new MemoryStream(compBytes);
            using var ds = new ZLibStream(ms, CompressionMode.Decompress);
            ds.CopyTo(decompressed);
        }
        var events = new List<ReplayEvent>();
        if (decompressed.Length > 0)
        {
            decompressed.Position = 0;
            using var br = new BinaryReader(decompressed, Encoding.ASCII, leaveOpen: true);
            if (br.BaseStream.Length >= 4)
                br.ReadUInt32(); // skip constant
            uint current = 0;
            while (br.BaseStream.Position < br.BaseStream.Length)
            {
                byte blockId = br.ReadByte();
                switch (blockId)
                {
                    case 0x1F:
                    case 0x1E:
                        if (br.BaseStream.Position + 4 > br.BaseStream.Length)
                            return new ReplayInfo(gameId, version, build, length, events);
                        ushort blockLen = br.ReadUInt16();
                        ushort timeInc = br.ReadUInt16();
                        current += timeInc;
                        int read = 0;
                        while (read < blockLen - 2)
                        {
                            byte playerId = br.ReadByte();
                            ushort actionLen = br.ReadUInt16();
                            read += 3;
                            var dataBytes = br.ReadBytes(actionLen);
                            read += actionLen;
                            using var actionMs = new MemoryStream(dataBytes);
                            using var actionReader = new BinaryReader(actionMs);
                            var action = ActionParser.Parse(actionReader.ReadByte(), actionReader);
                            events.Add(new ReplayEvent(current, playerId, action));
                        }
                        break;
                    case 0x20:
                        byte player = br.ReadByte();
                        ushort byteCount = br.ReadUInt16();
                        byte flags = br.ReadByte();
                        uint mode = 0;
                        if (flags == 0x20)
                            mode = br.ReadUInt32();
                        string msg = ReadZeroTerminatedString(br);
                        events.Add(new ReplayEvent(current, player, new ChatMessageAction(mode, 0, msg)));
                        break;
                    case 0x17:
                        br.ReadBytes(4); // reason
                        br.ReadByte();    // playerId
                        br.ReadBytes(4);  // result
                        br.ReadUInt32();
                        break;
                    case 0x1A:
                    case 0x1B:
                    case 0x1C:
                        br.ReadUInt32();
                        break;
                    case 0x22:
                        byte len = br.ReadByte();
                        br.ReadBytes(len);
                        break;
                    case 0x23:
                        br.ReadBytes(10);
                        break;
                    case 0x2F:
                        br.ReadBytes(8);
                        break;
                    default:
                        // skip unknown block id; format not handled
                        break;
                }
            }
        }

        return new ReplayInfo(gameId, version, build, length, events);
    }

    private static string ReadZeroTerminatedString(BinaryReader br)
    {
        List<byte> bytes = new();
        byte b;
        while ((b = br.ReadByte()) != 0)
            bytes.Add(b);
        return Encoding.UTF8.GetString(bytes.ToArray());
    }
}
