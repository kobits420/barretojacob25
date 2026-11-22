using UnityEngine;

/// <summary>
/// Hit target on the door plate. The puzzle itself lives on <see cref="DoorLockView"/>.
/// </summary>
public sealed class CodeLockTerminal : MonoBehaviour
{
    [SerializeField] DoorLockView door;

    public DoorLockView Door => door;
}
