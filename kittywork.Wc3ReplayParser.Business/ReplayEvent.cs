using System;

namespace kittywork.Wc3ReplayParser.Business;

public record ReplayEvent(
    uint TimeMs,
    byte PlayerId,
    ReplayAction Action)
{
    public override string ToString()
    {
        var ts = TimeSpan.FromMilliseconds(TimeMs);
        var timeText = ts.ToString(@"hh\:mm\:ss\.fff");
        return $"{timeText} Player {PlayerId}: {Action.Explain()}";
    }
}
