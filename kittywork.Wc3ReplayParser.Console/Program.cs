using kittywork.Wc3ReplayParser.Business;

if (args.Length == 0)
{
    Console.WriteLine("Usage: provide path to .w3g file");
    return;
}

var parser = new ReplayParser();
var info = parser.Parse(args[0]);
var duration = TimeSpan.FromMilliseconds(info.GameLengthMs);
Console.WriteLine($"Game: {info.GameId} Version: {info.Version} Build: {info.Build} Length: {duration:hh\\:mm\\:ss}");
foreach (var e in info.Events)
{
    Console.WriteLine(e.ToString());
}
Console.WriteLine($"Summary: {info.Events.Count} events, duration {duration:hh\\:mm\\:ss}");
