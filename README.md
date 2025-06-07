# kittywork-wc3-replayparser

This repository contains example .NET 9 projects demonstrating how to parse Warcraft 3 replay files. The main implementation lives in `kittywork.Wc3ReplayParser.Business` with a simple console application in `kittywork.Wc3ReplayParser.Console`.

Run unit tests with:

```bash
dotnet test kittywork.Wc3ReplayParser.sln
```

The console application prints the replay header and lists all parsed action blocks with timestamps.

