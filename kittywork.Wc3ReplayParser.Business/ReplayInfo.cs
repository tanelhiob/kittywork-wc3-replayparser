namespace kittywork.Wc3ReplayParser.Business;

public record ReplayInfo(
    string GameId,
    uint Version,
    ushort Build,
    uint GameLengthMs,
    IReadOnlyList<ReplayEvent> Events);
