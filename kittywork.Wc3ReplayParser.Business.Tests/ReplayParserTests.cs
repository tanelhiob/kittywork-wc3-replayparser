using System.IO.Compression;
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
        Assert.Empty(info.Events);
    }

    [Fact]
    public void Parse_WithEvents_ParsesEvents()
    {
        var data = CreateReplayWithEvent();
        using var ms = new MemoryStream(data);
        var parser = new ReplayParser();
        var info = parser.Parse(ms);
        Assert.Single(info.Events);
        var evt = info.Events[0];
        Assert.Equal(1000u, evt.TimeMs);
        Assert.Equal((byte)0, evt.PlayerId);
        Assert.Equal(new byte[]{0x7B}, evt.Data);
    }

    private static byte[] CreateTestReplay()
    {
        var buffer = new List<byte>();
        buffer.AddRange(Encoding.ASCII.GetBytes("Warcraft III recorded game"));
        buffer.Add(0x1A);
        buffer.Add(0x00);
        buffer.AddRange(BitConverter.GetBytes((uint)0x44));
        buffer.AddRange(BitConverter.GetBytes((uint)0));
        buffer.AddRange(BitConverter.GetBytes((uint)1));
        buffer.AddRange(BitConverter.GetBytes((uint)0));
        buffer.AddRange(BitConverter.GetBytes((uint)0));
        buffer.AddRange(Encoding.ASCII.GetBytes("W3XP"));
        buffer.AddRange(BitConverter.GetBytes((uint)0x00010000));
        buffer.AddRange(BitConverter.GetBytes((ushort)1));
        buffer.AddRange(BitConverter.GetBytes((ushort)0x8000));
        buffer.AddRange(BitConverter.GetBytes((uint)0));
        buffer.AddRange(BitConverter.GetBytes((uint)0));
        return buffer.ToArray();
    }

    private static byte[] CreateReplayWithEvent()
    {
        var decompressed = new List<byte>();
        decompressed.AddRange(new byte[]{0,0,0,0});
        decompressed.Add(0x1F);
        decompressed.AddRange(BitConverter.GetBytes((ushort)6));
        decompressed.AddRange(BitConverter.GetBytes((ushort)1000));
        decompressed.Add(0x00);
        decompressed.AddRange(BitConverter.GetBytes((ushort)1));
        decompressed.Add(0x7B);
        var uncompressedBytes = decompressed.ToArray();
        var compMs = new MemoryStream();
        using(var ds = new DeflateStream(compMs, CompressionLevel.Optimal, true))
            ds.Write(uncompressedBytes,0,uncompressedBytes.Length);
        compMs.Position = 0;
        var compBytes = compMs.ToArray();
        var block = new List<byte>();
        block.AddRange(BitConverter.GetBytes((ushort)compBytes.Length));
        block.AddRange(BitConverter.GetBytes((ushort)uncompressedBytes.Length));
        block.AddRange(new byte[4]);
        block.AddRange(compBytes);
        var header = new List<byte>();
        header.AddRange(Encoding.ASCII.GetBytes("Warcraft III recorded game"));
        header.Add(0x1A);
        header.Add(0x00);
        header.AddRange(BitConverter.GetBytes((uint)0x44));
        header.AddRange(BitConverter.GetBytes((uint)block.Count));
        header.AddRange(BitConverter.GetBytes((uint)1));
        header.AddRange(BitConverter.GetBytes((uint)uncompressedBytes.Length));
        header.AddRange(BitConverter.GetBytes((uint)1));
        header.AddRange(Encoding.ASCII.GetBytes("W3XP"));
        header.AddRange(BitConverter.GetBytes((uint)0x00010000));
        header.AddRange(BitConverter.GetBytes((ushort)1));
        header.AddRange(BitConverter.GetBytes((ushort)0x8000));
        header.AddRange(BitConverter.GetBytes((uint)1000));
        header.AddRange(BitConverter.GetBytes((uint)0));
        header.AddRange(block);
        return header.ToArray();
    }
}

