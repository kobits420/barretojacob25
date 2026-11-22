using System;
using JacobsEscape.PuzzleCore;

static class Program
{
    static int failures;

    static int Main()
    {
        Check(DoorLockPuzzle.FridgeNoteText == "Sometimes my memory gets a little hazy...174435754", "fridge note text");
        Check(DoorLockPuzzle.FridgeNoteText.Contains(DoorLockPuzzle.CorrectCode), "note shows the code");
        Check(DoorLockPuzzle.CorrectCode == "174435754", "correct code");

        var puzzle = new DoorLockPuzzle();
        Check(puzzle.Submit("174435753") == DoorLockSubmitResult.Rejected, "off-by-one rejected");
        Check(!puzzle.IsUnlocked, "off-by-one stays locked");
        Check(puzzle.Entry == "", "rejected entry is cleared");

        Check(puzzle.Submit("17443575") == DoorLockSubmitResult.Rejected, "prefix rejected");
        Check(puzzle.Submit("1744357540") == DoorLockSubmitResult.Rejected, "longer code rejected");
        Check(puzzle.Submit("") == DoorLockSubmitResult.Rejected, "empty rejected");
        Check(puzzle.Submit(null) == DoorLockSubmitResult.Rejected, "null rejected");
        Check(puzzle.Submit(" 174435754") == DoorLockSubmitResult.Rejected, "padded code rejected");
        Check(!puzzle.IsUnlocked, "still locked after wrong codes");

        foreach (char digit in DoorLockPuzzle.CorrectCode)
            puzzle.AppendDigit(digit);
        Check(puzzle.Entry == "174435754", "digits build the code");
        Check(puzzle.Submit() == DoorLockSubmitResult.Unlocked, "entry submit unlocks");
        Check(puzzle.IsUnlocked, "unlocked flag");
        Check(puzzle.Submit("000") == DoorLockSubmitResult.AlreadyUnlocked, "stays unlocked");
        Check(puzzle.IsUnlocked, "cannot relock");

        var other = new DoorLockPuzzle();
        other.AppendDigit('x');
        Check(other.Entry == "", "non-digit ignored");
        other.AppendDigit('1');
        other.AppendDigit('2');
        other.Backspace();
        Check(other.Entry == "1", "backspace");
        other.Clear();
        Check(other.Entry == "", "clear");

        var capped = new DoorLockPuzzle();
        for (int i = 0; i < DoorLockPuzzle.MaxEntryLength + 4; i++)
            capped.AppendDigit('9');
        Check(capped.Entry.Length == DoorLockPuzzle.MaxEntryLength, "entry length cap");
        Check(capped.Submit() == DoorLockSubmitResult.Rejected, "capped wrong code rejected");

        if (failures == 0)
        {
            Console.WriteLine("DoorLockPuzzle tests passed.");
            return 0;
        }

        Console.Error.WriteLine(failures + " failed.");
        return 1;
    }

    static void Check(bool condition, string name)
    {
        if (condition) return;
        failures++;
        Console.Error.WriteLine("FAIL " + name);
    }
}
