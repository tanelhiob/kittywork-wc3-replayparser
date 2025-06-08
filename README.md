# kittywork-wc3-replayparser

This repository contains example .NET 9 projects demonstrating how to parse Warcraft 3 replay files. The main implementation lives in `kittywork.Wc3ReplayParser.Business` with a simple console application in `kittywork.Wc3ReplayParser.Console`.

Run unit tests with:

```bash
dotnet test kittywork.Wc3ReplayParser.sln
```

The console application prints the replay header and lists all parsed action blocks with timestamps.

## Unknown actions

Running the console against the sample replays reveals several opcode IDs that
the parser does not recognize. Example IDs are `0x00`, `0x02`, `0x1A`, `0x61`,
`0x66`, `0x67`, `0x68`, `0x6A`, `0x7A` and `0xBC`. Web searches did not uncover
documentation for these codes so their semantics remain unclear. They appear in
the output as `UnknownAction` entries. Contributions that clarify these opcodes
are welcome.

