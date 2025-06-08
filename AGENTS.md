# Repository usage instructions

This repository targets .NET 9 and contains a replay parser library with a console application.

## Build
```bash
dotnet build kittywork.Wc3ReplayParser.sln
```

## Test
```bash
dotnet test kittywork.Wc3ReplayParser.sln
```

Example replay files are available in the `testdata` directory. You can pass one
of them to the console application when testing it locally.

## Run console
Provide a path to a `.w3g` file when running:
```bash
dotnet run --project kittywork.Wc3ReplayParser.Console -- <path-to-replay>
```

## Architecture
The code is organized into a few simple projects:

- **kittywork.Wc3ReplayParser.Business** – the library that implements
  `ReplayParser`. It exposes `IReplayParser` with methods to parse a file or
  stream. `ReplayParser` reads the replay header, decompresses the action
  blocks using `ZLibStream` and maps raw action bytes into strongly typed
  `ReplayAction` records via `ActionParser`. Parsed information is returned as a
  `ReplayInfo` record which contains metadata and a list of `ReplayEvent`
  entries.
- **kittywork.Wc3ReplayParser.Console** – a small demo application showing how
  to call the parser. It prints header values and each parsed event. Pass a
  path to a `.w3g` file from `testdata` when running.
- **kittywork.Wc3ReplayParser.Business.Tests** – xUnit tests for the business
  library. They validate parsing logic using generated replay data as well as
  real sample files from `testdata`.

This layout keeps the parsing logic independent from any UI while providing a
minimal console interface and automated tests.
