namespace kittywork.Wc3ReplayParser.Business;

public record ReplayEvent(
    uint TimeMs,
    byte PlayerId,
    ReplayAction Action)
{
    public override string ToString() => $"{TimeMs}ms Player {PlayerId}: {Action.Explain()}";
}
