# Settlement Planner

A small Farthest Frontier mod that adds a single icon button to the top bar
linking to the [Farthest Frontier Planner](https://sagedragoon79.github.io/FarthestFrontierPlanner/) —
a free in-browser city planner.

That's the entire feature set. One button, one click, opens the planner in
your default browser.

## Install

1. Install [MelonLoader](https://melonwiki.xyz/) for Farthest Frontier (Mono build).
2. Drop `SettlementPlanner.dll` into `<Farthest Frontier>\Farthest Frontier (Mono)\Mods\`.
3. Launch the game — the planner icon appears on the top bar right after the
   Food counter.

## Coexistence with Keep Clarity

[Keep Clarity](https://github.com/sagedragoon79/KeepClarity) ships an
equivalent planner button as part of its broader UI overhaul. If both mods
are installed, **Settlement Planner detects Keep Clarity at startup and
suppresses its own button** so you only see one. Keep Clarity's version
wins because it was authored alongside the rest of the top-bar overhaul.

## Build from source

```
dotnet build src/SettlementPlanner.csproj -p:GameDir="<path-to-Farthest-Frontier-game-folder>"
```

Output lands in `src/bin/Debug/net46/SettlementPlanner.dll`.

## Author

[SageDragoon](https://github.com/sagedragoon79)
