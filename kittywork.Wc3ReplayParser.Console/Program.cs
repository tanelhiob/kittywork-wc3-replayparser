using kittywork.Wc3ReplayParser.Business;

if (args.Length == 0)
{
    Console.WriteLine("Usage: provide path to .w3g file");
    return;
}

var parser = new ReplayParser();
var info = parser.Parse(args[0]);
Console.WriteLine($"Game: {info.GameId} Version: {info.Version} Build: {info.Build} Length(ms): {info.GameLengthMs}");
