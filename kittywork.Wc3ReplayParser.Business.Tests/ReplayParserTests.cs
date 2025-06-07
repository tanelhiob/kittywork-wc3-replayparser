using System.Text;
using kittywork.Wc3ReplayParser.Business;

namespace kittywork.Wc3ReplayParser.Business.Tests;

public class ReplayParserTests
{
    [Fact]
    public void Parse_Stream_ReturnsHeaderInfo()
    {
        var data = CreateTestReplay();
        using var ms = new MemoryStream(data);
        var parser = new ReplayParser();
        var info = parser.Parse(ms);
        Assert.Equal("W3XP", info.GameId);
        Assert.Equal(0x00010000u, info.Version);
        Assert.Equal((ushort)1, info.Build);
        Assert.Equal(0u, info.GameLengthMs);
    }

    private static byte[] CreateTestReplay()
    {
        var buffer = new List<byte>();
        buffer.AddRange(Encoding.ASCII.GetBytes("Warcraft III recorded game"));
        buffer.Add(0x1A);
        buffer.Add(0x00);
        buffer.AddRange(BitConverter.GetBytes((uint)0x44)); // header size
        buffer.AddRange(BitConverter.GetBytes((uint)0)); // compressed size
        buffer.AddRange(BitConverter.GetBytes((uint)1)); // header version
        buffer.AddRange(BitConverter.GetBytes((uint)0)); // decompressed size
        buffer.AddRange(BitConverter.GetBytes((uint)0)); // blocks
        buffer.AddRange(Encoding.ASCII.GetBytes("W3XP"));
        buffer.AddRange(BitConverter.GetBytes((uint)0x00010000)); // version
        buffer.AddRange(BitConverter.GetBytes((ushort)1)); // build
        buffer.AddRange(BitConverter.GetBytes((ushort)0x8000)); // flags
        buffer.AddRange(BitConverter.GetBytes((uint)0)); // length
        buffer.AddRange(BitConverter.GetBytes((uint)0)); // crc
        return buffer.ToArray();
    }
}
