using System.Linq;
using kittywork.Wc3ReplayParser.Business;

namespace kittywork.Wc3ReplayParser.Business.Tests;

public class ReplayInfoExtensionsTests
{
    [Fact]
    public void GetUnknownEvents_ReturnsUnknownActions()
    {
        var path = Path.GetFullPath(Path.Combine(AppContext.BaseDirectory, "../../../..", "testdata/w3c-20250511132852.w3g"));
        var parser = new ReplayParser();
        var info = parser.Parse(path);
        var unknownEvents = info.GetUnknownEvents().ToList();
        Assert.NotEmpty(unknownEvents);
        Assert.All(unknownEvents, e => Assert.IsType<UnknownAction>(e.Action));
    }

    [Fact]
    public void Parse_RealReplay_ParsesChatMessages()
    {
        var path = Path.GetFullPath(Path.Combine(AppContext.BaseDirectory, "../../../..", "testdata/w3c-20250511145524.w3g"));
        var parser = new ReplayParser();
        var info = parser.Parse(path);
        Assert.Contains(info.Events, e => e.Action is ChatMessageAction);
        Assert.Contains(info.Events, e => e.Action is MinimapPingAction);
        Assert.Contains(info.Events, e => e.Action is MmdMessageAction);
    }
}
