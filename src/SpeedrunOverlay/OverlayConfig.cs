using BepInEx.Configuration;
using UnityEngine;

namespace SpeedrunOverlay;

public sealed class OverlayConfig
{
    public OverlayConfig(ConfigFile file)
    {
        // BepInEx 6.0.0-be.733 does not ship KeyboardShortcut. KeyCode entries are the bind.
        Confirm = file.Bind("Keys", "Confirm", KeyCode.F8,
            "Advance the current step. Required for manual steps, and for a check that was already true when the step became current.");
        Back = file.Bind("Keys", "Back", KeyCode.F7,
            "Return to the previous step. A check that is already true will not auto-advance again until you confirm.");
        ToggleOverlay = file.Bind("Keys", "ToggleOverlay", KeyCode.F6,
            "Show or hide the step overlay. The path and highlights keep their own toggles.");
        TogglePath = file.Bind("Keys", "TogglePath", KeyCode.F9,
            "Show or hide the world path line.");
        ToggleHighlights = file.Bind("Keys", "ToggleHighlights", KeyCode.F10,
            "Show or hide highlights for the nodes, V Bloods, and mobs this step names.");
        PinTarget = file.Bind("Keys", "PinTarget", KeyCode.F11,
            "Save the player's current world position as this step's path target.");

        ShowOverlay = file.Bind("General", "ShowOverlay", true, "Overlay visibility. The toggle key flips this.");
        ShowPath = file.Bind("General", "ShowPath", true, "Path line visibility. The line stays off until the step is pinned.");
        ShowHighlights = file.Bind("General", "ShowHighlights", true, "Highlight visibility.");
        CurrentStep = file.Bind("General", "CurrentStep", 1, "1-based route step. Updated as you advance.");
    }

    public ConfigEntry<KeyCode> Confirm { get; }
    public ConfigEntry<KeyCode> Back { get; }
    public ConfigEntry<KeyCode> ToggleOverlay { get; }
    public ConfigEntry<KeyCode> TogglePath { get; }
    public ConfigEntry<KeyCode> ToggleHighlights { get; }
    public ConfigEntry<KeyCode> PinTarget { get; }
    public ConfigEntry<bool> ShowOverlay { get; }
    public ConfigEntry<bool> ShowPath { get; }
    public ConfigEntry<bool> ShowHighlights { get; }
    public ConfigEntry<int> CurrentStep { get; }

    public static bool Pressed(KeyCode key) => key != KeyCode.None && Input.GetKeyDown(key);
}
