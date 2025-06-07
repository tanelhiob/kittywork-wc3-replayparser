namespace kittywork.Wc3ReplayParser.Business;

public record ReplayEvent(
    uint TimeMs,
    byte PlayerId,
    byte[] Data);
