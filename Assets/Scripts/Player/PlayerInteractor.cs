using UnityEngine;

public sealed class PlayerInteractor : MonoBehaviour
{
    [SerializeField] Transform eye;
    [SerializeField] CodeLockPanel lockPanel;
    [SerializeField] float reach = 2.4f;

    void Awake()
    {
        if (eye == null || lockPanel == null)
        {
            Debug.LogError("PlayerInteractor is missing an eye or a code lock panel.");
            enabled = false;
        }
    }

    void Update()
    {
        if (lockPanel.IsOpen) return;

        string hint = "";
        Ray ray = new Ray(eye.position, eye.forward);
        if (Physics.Raycast(ray, out RaycastHit hit, reach, Physics.DefaultRaycastLayers, QueryTriggerInteraction.Ignore)
            && hit.collider.TryGetComponent(out CodeLockTerminal terminal)
            && terminal.Door != null)
        {
            if (terminal.Door.Puzzle.IsUnlocked)
            {
                hint = "Door unlocked";
            }
            else
            {
                hint = "E  -  code lock";
                if (Input.GetKeyDown(KeyCode.E))
                    lockPanel.Open(terminal.Door.Puzzle);
            }
        }

        lockPanel.SetHint(hint);
    }
}
