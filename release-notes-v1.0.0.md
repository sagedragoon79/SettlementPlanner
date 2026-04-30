# Settlement Planner v1.0.0 — initial release

A small, single-purpose Farthest Frontier mod: adds one icon button to the
top bar that opens the [Farthest Frontier Planner](https://sagedragoon79.github.io/FarthestFrontierPlanner/)
web tool in your default browser.

That's it. No other features. If you want a fuller UI overhaul, see [Keep Clarity](https://github.com/sagedragoon79/KeepClarity).

## Features

- One icon-only button on the top bar, right after the Food counter
- Hover tooltip: "Click to open the Farthest Frontier Planner in your browser."
- Click → opens the planner in your default browser via `Application.OpenURL`

## Coexistence with Keep Clarity

If Keep Clarity is also installed, Settlement Planner detects it at startup
and suppresses its own button — Keep Clarity already ships an equivalent.
Only one button will render.

## Install

1. [MelonLoader](https://melonwiki.xyz/) for Farthest Frontier (Mono build).
2. Drop `SettlementPlanner.dll` into `<game>\Farthest Frontier (Mono)\Mods\`.
3. Launch.
