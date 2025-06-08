using System.Text;
using kittywork.Wc3ReplayParser.Business;

namespace kittywork.Wc3ReplayParser.Business.Tests;

public class ActionParserExtensionsTests
{
    [Fact]
    public void TryParseAction_TransferResources_Succeeds()
    {
        var buffer = new byte[]
        {
            0x51,
            0x02,
            0x01,0x00,0x00,0x00,
            0x02,0x00,0x00,0x00
        };
        ReadOnlySpan<byte> span = buffer;
        Assert.True(ReplayActionParserExtensions.TryParseAction(ref span, out var action));
        var tr = Assert.IsType<TransferResourcesAction>(action);
        Assert.Equal(2, tr.Slot);
        Assert.Equal(1u, tr.Gold);
        Assert.Equal(2u, tr.Lumber);
        Assert.True(span.IsEmpty); // all bytes consumed
    }

    [Fact]
    public void TryParseAction_NotEnoughBytes_ReturnsFalse()
    {
        var buffer = new byte[] { 0x51, 0x02, 0x01 };
        ReadOnlySpan<byte> span = buffer;
        Assert.False(ReplayActionParserExtensions.TryParseAction(ref span, out _));
    }

    [Fact]
    public void TryParseAction_ChatMessage_Succeeds()
    {
        var msg = "hello";
        var buf = new List<byte>();
        buf.Add(0x60);
        buf.AddRange(BitConverter.GetBytes((uint)1));
        buf.AddRange(BitConverter.GetBytes((uint)2));
        buf.AddRange(Encoding.UTF8.GetBytes(msg));
        buf.Add(0);
        ReadOnlySpan<byte> span = buf.ToArray();
        Assert.True(ReplayActionParserExtensions.TryParseAction(ref span, out var action));
        var chat = Assert.IsType<ChatMessageAction>(action);
        Assert.Equal(msg, chat.Message);
        Assert.True(span.IsEmpty);
    }
}
