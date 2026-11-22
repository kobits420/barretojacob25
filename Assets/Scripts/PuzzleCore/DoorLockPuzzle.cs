using System;

namespace JacobsEscape.PuzzleCore
{
    public enum DoorLockSubmitResult
    {
        Rejected = 0,
        Unlocked = 1,
        AlreadyUnlocked = 2
    }

    /// <summary>
    /// Kitchen door code. This type owns the rule and the lock state.
    /// A later session can keep one instance and replicate it; nothing here
    /// knows about networking or UI.
    /// </summary>
    public sealed class DoorLockPuzzle
    {
        public const string CorrectCode = "174435754";
        public const string FridgeNoteText = "Sometimes my memory gets a little hazy...174435754";
        public const int MaxEntryLength = 16;

        string entry = "";

        public bool IsUnlocked { get; private set; }

        public string Entry => entry;

        public void AppendDigit(char digit)
        {
            if (IsUnlocked) return;
            if (digit < '0' || digit > '9') return;
            if (entry.Length >= MaxEntryLength) return;
            entry += digit;
        }

        public void Backspace()
        {
            if (IsUnlocked || entry.Length == 0) return;
            entry = entry.Substring(0, entry.Length - 1);
        }

        public void Clear()
        {
            if (IsUnlocked) return;
            entry = "";
        }

        public DoorLockSubmitResult Submit()
        {
            return Submit(entry);
        }

        public DoorLockSubmitResult Submit(string code)
        {
            if (IsUnlocked) return DoorLockSubmitResult.AlreadyUnlocked;
            if (string.Equals(code, CorrectCode, StringComparison.Ordinal))
            {
                IsUnlocked = true;
                entry = CorrectCode;
                return DoorLockSubmitResult.Unlocked;
            }

            entry = "";
            return DoorLockSubmitResult.Rejected;
        }
    }
}
