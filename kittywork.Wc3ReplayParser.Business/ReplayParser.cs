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
        ushort flags = 0;
        uint length = 0;
        uint crc = 0;

        if (headerVersion == 1)
        {
            gameId = Encoding.ASCII.GetString(reader.ReadBytes(4));
            version = reader.ReadUInt32();
            build = reader.ReadUInt16();
            flags = reader.ReadUInt16();
            length = reader.ReadUInt32();
            crc = reader.ReadUInt32();
        }

        return new ReplayInfo(gameId, version, build, length);
    }
}
