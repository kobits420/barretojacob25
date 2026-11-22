# V Rising speedrun overlay

Client-side BepInEx IL2CPP overlay for the 166-step speedrun route, including the end-game guide. It shows the current step, the coaching hint, and what completes the step. A world line runs from your character to a target you pin yourself. Nodes, V Bloods, and other prefabs that step cares about are highlighted when they are loaded nearby.

This plugin was compiled against the public client libraries. It has not been run inside the game.

## Install

1. Install [BepInExPack for V Rising](https://thunderstore.io/c/v-rising/p/BepInEx/BepInExPack_V_Rising/) (BepInEx `6.0.0-be.733`) into the V Rising **client** folder (`VRising`, not `VRisingServer`).
2. Build this plugin, or take `SpeedrunOverlay.dll` from a Release build:

   ```bash
   dotnet build src/SpeedrunOverlay/SpeedrunOverlay.csproj -c Release
   ```

   The project targets `net6.0` and references `VRising.Unhollowed.Client` `1.1.9.9219901`.
3. Copy `src/SpeedrunOverlay/bin/Release/net6.0/SpeedrunOverlay.dll` to `BepInEx/plugins`.
4. Start the game and load a character. The overlay sits at the top left.

Configs are created on first launch:

- `BepInEx/config/kobits420.SpeedrunOverlay.cfg` — keys, visibility, and the saved step number
- `BepInEx/config/kobits420.SpeedrunOverlay.pins.cfg` — one `step=x,y,z` line per pinned target

## Controls

Defaults are single keys. Change them in the cfg under `[Keys]`.

| Key | Action |
| --- | --- |
| F8 | Confirm and advance. This is how every manual step moves on. |
| F7 | Previous step. If that step's check is already true, it will not jump forward again until you confirm. |
| F6 | Show or hide the overlay. |
| F9 | Show or hide the path line. |
| F10 | Show or hide highlights. |
| F11 | Pin the active step's target to where you are standing. |

## Path line

The route has no map coordinates, and this mod does not invent any. The line stays off, and the overlay says so, until you stand on the spot you want and press **F11**. That writes the active step's world position into the pins file. The line is drawn from your character to that pin. Each step has its own pin.

## What auto-completes

A step advances by itself only when every gate on it is a live verifier and the check **becomes true while that step is current**. If it was already true when you arrived (a boss you already unlocked, a heart you already upgraded), press F8. That keeps a geared character from skipping the route, and it keeps a re-kill from completing itself off the first unlock.

Conditional lines stay until you confirm. They are not skipped: check boxes/horses, shops if you have coins, sulphur if it is there, ghost crystals if you see them, and research a weapon if you can.

Rotations, "check" lines, generic "Base" / "store materials" / "base management", room shapes, and places that are not a whole region (Dawnbreak, Rustlock, caves, markets, Brighthaven) are manual. Pin them if you want the line.

### Verified against the 1.1.9 client libraries

These call types that exist in `VRising.Unhollowed.Client` 1.1.9.9219901. If a read throws in a newer game build, that probe turns itself off and the steps that need it wait for F8. The plugin still loads.

| Check | Hook |
| --- | --- |
| Gear score | `Equipment.GetFullLevel` on the local character (`ConsoleShared.TryGetLocalCharacterInCurrentWorld`, `World.FindWorld(WorldType.Client)`) |
| V Blood, first unlock | `ProgressionUtility.HasUnlockedProgression` |
| Region | `CurrentWorldRegion` on the local character or user. Oakveil is `WorldRegionType.Strongblade` in these libraries. Gloomrot matches South or North. |
| Castle Heart level | `CastleHeart.Level` on a heart whose `LastUserOwner` or team is yours |
| Inventory | `InventoryBuffer` on the local character and user |
| Equipped | `Equipment.IsEquipped` |
| Placed structure | `PrefabGUID` plus `CastleHeartConnection` to one of your hearts |
| Quest | `AchievementClaimedElement` (Waygate, and Building a Castle for the level-10 step) |
| Position, path, highlights | `LocalTransform` |

Prefab hashes come from the published V Blood unit list and the vrisingmods prefab tables. Highlights are the nearest loaded matches within 280 meters (bosses, ore nodes, cotton, sunflower, emery, ghost crystal, blood crystal).

### Stubbed, so the step stays on confirm

| Check | Why it is not a gate |
| --- | --- |
| V Blood kill event / re-kill | `VBloodConsumed` is not in the 1.1.9 client assemblies. Goreswine, Magnus, and Baron the second time stay on F8. `KillEventVerifier` never passes. |
| Named spells | `SpellSchoolProgressionUtility_Shared.IsAbilityUnlocked` exists, but Frost Counter, Frost Barrier, Ward of the Damned, and Crimson Beam hashes were not in the confirmed tables. They are on-screen hints. `SpellUnlockVerifier` never passes. |
| Sub-zone and "did the rotation" | No coordinates in the strat, and no confirmed subzone id. |
| Hollowfang, Dawnthorn, Scourge Pendant, Eye of Mortium | Those names were not in the confirmed item table, so the steps are not bound to a guessed hash. |
| Horse, tamed horse, caravan, room shape, rift farm, best-in-slot | No confirmed component that means "done". |

## Route

Every numbered line is its own step, from gear level 10 through the Mortium rift farm. Coaching notes (line-of-sight elementals, Frost Counter, Ward of the Damned, Frost Barrier, the temp-base teleport) are the hint line under the step.
