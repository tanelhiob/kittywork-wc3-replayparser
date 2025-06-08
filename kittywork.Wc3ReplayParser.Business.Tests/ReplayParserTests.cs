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
        Assert.IsType<TransferResourcesAction>(evt.Action);
        var tr = (TransferResourcesAction)evt.Action;
        Assert.Equal(2, tr.Slot);
        Assert.Equal(1u, tr.Gold);
        Assert.Equal(2u, tr.Lumber);
    }

    [Fact]
    public void Parse_ChatMessageBlock_ParsesMessage()
    {
        var data = CreateReplayWithChatBlock();
        using var ms = new MemoryStream(data);
        var parser = new ReplayParser();
        var info = parser.Parse(ms);
        Assert.Single(info.Events);
        var evt = info.Events[0];
        Assert.Equal((byte)0, evt.PlayerId);
        Assert.IsType<ChatMessageAction>(evt.Action);
        var chat = (ChatMessageAction)evt.Action;
        Assert.Equal("hello", chat.Message);
    }

    [Theory]
    [InlineData("testdata/w3c-20250511132852.w3g")]
    [InlineData("testdata/w3c-20250511135035.w3g")]
    [InlineData("testdata/w3c-20250511135332.w3g")]
    [InlineData("testdata/w3c-20250511141650.w3g")]
    [InlineData("testdata/w3c-20250511145524.w3g")]
    [InlineData("testdata/w3c-20250511151824.w3g")]
    public void Parse_RealReplays_Success(string path)
    {
        var fullPath = Path.GetFullPath(Path.Combine(AppContext.BaseDirectory, "../../../..", path));
        var parser = new ReplayParser();
        var info = parser.Parse(fullPath);
        Assert.False(string.IsNullOrEmpty(info.GameId));
        Assert.NotNull(info.Events);
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
        var action = new List<byte>();
        action.Add(0x51); // action id
        action.Add(0x02); // slot
        action.AddRange(BitConverter.GetBytes((uint)1));
        action.AddRange(BitConverter.GetBytes((uint)2));
        ushort blockLen = (ushort)(2 + 3 + action.Count);
        decompressed.AddRange(BitConverter.GetBytes(blockLen));
        decompressed.AddRange(BitConverter.GetBytes((ushort)1000));
        decompressed.Add(0x00);
        decompressed.AddRange(BitConverter.GetBytes((ushort)action.Count));
        decompressed.AddRange(action);
        var uncompressedBytes = decompressed.ToArray();
        var compMs = new MemoryStream();
        using(var ds = new ZLibStream(compMs, CompressionLevel.Optimal, leaveOpen: true))
            ds.Write(uncompressedBytes,0,uncompressedBytes.Length);
        compMs.Position = 0;
        var compBytes = compMs.ToArray();
        var block = new List<byte>();
        block.AddRange(BitConverter.GetBytes((uint)compBytes.Length));
        block.AddRange(BitConverter.GetBytes((uint)uncompressedBytes.Length));
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

    private static byte[] CreateReplayWithChatBlock()
    {
        var decompressed = new List<byte>();
        decompressed.AddRange(new byte[] { 0, 0, 0, 0 });
        decompressed.Add(0x20); // chat block
        byte playerId = 0;
        string msg = "hello";
        byte flags = 0;
        ushort byteCount = (ushort)(1 + msg.Length + 1);
        decompressed.Add(playerId);
        decompressed.AddRange(BitConverter.GetBytes(byteCount));
        decompressed.Add(flags);
        decompressed.AddRange(Encoding.UTF8.GetBytes(msg));
        decompressed.Add(0x00);

        var uncompressedBytes = decompressed.ToArray();
        var compMs = new MemoryStream();
        using (var ds = new ZLibStream(compMs, CompressionLevel.Optimal, leaveOpen: true))
            ds.Write(uncompressedBytes, 0, uncompressedBytes.Length);
        compMs.Position = 0;
        var compBytes = compMs.ToArray();

        var block = new List<byte>();
        block.AddRange(BitConverter.GetBytes((uint)compBytes.Length));
        block.AddRange(BitConverter.GetBytes((uint)uncompressedBytes.Length));
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
        header.AddRange(BitConverter.GetBytes((uint)0));
        header.AddRange(BitConverter.GetBytes((uint)0));
        header.AddRange(block);
        return header.ToArray();
    }
}

