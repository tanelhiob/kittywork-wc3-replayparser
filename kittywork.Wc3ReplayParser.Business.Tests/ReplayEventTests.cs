using System.IO.Compression;
using System.Text;
using kittywork.Wc3ReplayParser.Business;

namespace kittywork.Wc3ReplayParser.Business.Tests;

public class ReplayEventTests
{
    [Fact]
    public void ToString_FormatsTimeAsHumanReadable()
    {
        var data = CreateReplayWithEvent();
        using var ms = new MemoryStream(data);
        var parser = new ReplayParser();
        var info = parser.Parse(ms);
        var evt = info.Events[0];
        Assert.StartsWith("00:00:01.000", evt.ToString());
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
}
