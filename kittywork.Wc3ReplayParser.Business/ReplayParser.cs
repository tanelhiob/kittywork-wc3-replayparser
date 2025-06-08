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
            ushort compLen = reader.ReadUInt16();
            ushort decompLen = reader.ReadUInt16();
            reader.ReadUInt32(); // checksum
            var compBytes = reader.ReadBytes(compLen);
            using var ms = new MemoryStream(compBytes);
            using var ds = new DeflateStream(ms, CompressionMode.Decompress);
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
                if (blockId != 0x1F && blockId != 0x1E)
                    break;
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
            }
        }

        return new ReplayInfo(gameId, version, build, length, events);
    }
}
