# Jacob's Escape

Unity 6 greybox. Puzzle 1 is the kitchen code lock.

## Open

1. Install Unity **6000.6.10f1** (Unity 6.6) or a newer Unity 6. Hub will offer to upgrade if your Editor is a later 6.x.
2. Add this repository folder as a project. The Unity project root is the repository root.
3. Open `Assets/Scenes/Kitchen.unity` if it is not already the active scene.
4. Press Play.

The project uses the built-in render pipeline, so it opens without downloading the Universal Render Pipeline package. On the first open, a fresh project shows an untitled scene; an editor script then opens the kitchen.

## Try the lock

- WASD moves. The mouse looks. Click the game view if the cursor is free. Esc releases the cursor.
- The fridge is on the left. Its sticky note reads: `Sometimes my memory gets a little hazy...174435754`
- Walk to the dark plate on the door and press E.
- Enter `174435754` and press Enter (or the Enter button). The door swings into the kitchen.
- Any other code leaves the door locked.

`DoorLockPuzzle` (`Assets/Scripts/PuzzleCore`) owns the code and whether the door is unlocked. The keypad only sends digits and reads that state. This puzzle does not add networking.

When you press Play, the grey boxes for the fridge, counter, and table are replaced with furniture from Kenney's Furniture Kit (CC0). The license note is `Assets/Art/KenneyFurnitureKit-CC0.txt`. The door, the code lock, and the fridge note stay the same.

If the door swings out of the kitchen instead of into it, select `DoorHinge` and set Open Angle to `-90`.

## What was exercised without the Editor

`Tools/DoorLockPuzzleTests` checks the lock rule (the note text, the correct code, and rejected codes). The kitchen scene was hand-authored and has not been run in Play mode.
