using System;
using JacobsEscape.PuzzleCore;
using UnityEngine;

/// <summary>
/// Shows the kitchen door from <see cref="DoorLockPuzzle"/> state.
/// The hinge this component sits on swings when the puzzle unlocks.
/// </summary>
public sealed class DoorLockView : MonoBehaviour
{
    [SerializeField] float openAngle = 90f;
    [SerializeField] float swingSpeed = 140f;

    public DoorLockPuzzle Puzzle { get; private set; } = new DoorLockPuzzle();

    public void UsePuzzle(DoorLockPuzzle puzzle)
    {
        if (puzzle == null) throw new ArgumentNullException(nameof(puzzle));
        Puzzle = puzzle;
    }

    void Update()
    {
        float angle = Puzzle.IsUnlocked ? openAngle : 0f;
        Quaternion target = Quaternion.Euler(0f, angle, 0f);
        transform.localRotation = Quaternion.RotateTowards(
            transform.localRotation,
            target,
            swingSpeed * Time.deltaTime);
    }
}
