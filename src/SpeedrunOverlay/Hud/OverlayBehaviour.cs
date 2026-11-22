using SpeedrunOverlay.Game;
using SpeedrunOverlay.Route;
using UnityEngine;

namespace SpeedrunOverlay.Hud;

public sealed class OverlayBehaviour : MonoBehaviour
{
    private OverlayConfig? _config;
    private RouteRunner? _runner;
    private PinStore? _pins;
    private GameReader? _reader;
    private WorldMarkers? _markers;
    private readonly GameSnapshot _snap = new();
    private float _nextRead;

    public void Bind(OverlayConfig config, RouteRunner runner, PinStore pins)
    {
        _config = config;
        _runner = runner;
        _pins = pins;
        _reader = new GameReader();
        _markers = new WorldMarkers();
    }

    public void OnDestroy()
    {
        _markers?.Destroy();
    }

    public void Update()
    {
        if (_config == null || _runner == null || _pins == null || _reader == null || _markers == null)
        {
            return;
        }

        if (OverlayConfig.Pressed(_config.ToggleOverlay.Value))
        {
            _config.ShowOverlay.Value = !_config.ShowOverlay.Value;
        }

        if (OverlayConfig.Pressed(_config.TogglePath.Value))
        {
            _config.ShowPath.Value = !_config.ShowPath.Value;
        }

        if (OverlayConfig.Pressed(_config.ToggleHighlights.Value))
        {
            _config.ShowHighlights.Value = !_config.ShowHighlights.Value;
        }

        if (OverlayConfig.Pressed(_config.Back.Value))
        {
            _runner.Back();
        }

        if (OverlayConfig.Pressed(_config.Confirm.Value))
        {
            _runner.Confirm();
        }

        if (Time.unscaledTime >= _nextRead)
        {
            _nextRead = Time.unscaledTime + 0.35f;
            var highlights = _config.ShowHighlights.Value ? _runner.Current.Highlights : null;
            _reader.Refresh(_snap, highlights);
            _runner.Tick(_snap);
        }

        if (OverlayConfig.Pressed(_config.PinTarget.Value) && _snap.PositionLive)
        {
            _pins.Set(_runner.Current.Number, _snap.X, _snap.Y, _snap.Z);
        }

        var pinned = _pins.TryGet(_runner.Current.Number, out var pin);
        _markers.DrawPath(
            _config.ShowPath.Value,
            _snap.PositionLive,
            _snap.X,
            _snap.Y,
            _snap.Z,
            pinned,
            pin);
        _markers.DrawHighlights(_config.ShowHighlights.Value, _snap.Marks);
    }

    public void OnGUI()
    {
        if (_config == null || _runner == null || _pins == null || !_config.ShowOverlay.Value)
        {
            DrawScreenMarks();
            return;
        }

        var step = _runner.Current;
        var pinned = _pins.TryGet(step.Number, out _);
        var pathLine = !_config.ShowPath.Value
            ? "Path hidden (" + Key(_config.TogglePath) + ")."
            : pinned
                ? "Path pinned for this step."
                : "Path off until you pin this step (" + Key(_config.PinTarget) + "). No coordinates are shipped with the route.";

        // Il2Cpp GUIStyle has no managed copy constructor, so the overlay uses the game skin.
        var area = new Rect(16f, 16f, 520f, 420f);
        GUI.Box(area, GUIContent.none);
        GUILayout.BeginArea(new Rect(28f, 24f, 496f, 400f));
        GUILayout.Label(Wrap($"Speedrun  {step.Number} / {_runner.Count}"));
        GUILayout.Label(Wrap(step.Text));
        if (!string.IsNullOrWhiteSpace(step.Hint))
        {
            GUILayout.Label(Wrap("Hint: " + step.Hint));
        }

        GUILayout.Label(Wrap("Completes when: " + step.CompletesWhen));
        GUILayout.Label(Wrap(_runner.GateStatus(_snap)));
        GUILayout.Label(Wrap(pathLine));
        if (_config.ShowHighlights.Value && step.Highlights.Length > 0)
        {
            GUILayout.Label(
                _snap.Marks.Count > 0
                    ? _snap.Marks.Count + " highlighted nearby."
                    : "No matching nodes or bosses are loaded nearby.");
        }

        GUILayout.Label(
            Key(_config.Confirm) + " confirm   " +
            Key(_config.Back) + " back   " +
            Key(_config.ToggleOverlay) + " overlay   " +
            Key(_config.TogglePath) + " path   " +
            Key(_config.ToggleHighlights) + " highlights   " +
            Key(_config.PinTarget) + " pin");
        GUILayout.EndArea();
        DrawScreenMarks();
    }

    private void DrawScreenMarks()
    {
        if (_config == null || !_config.ShowHighlights.Value || _snap.Marks.Count == 0)
        {
            return;
        }

        var camera = Camera.main;
        if (camera == null)
        {
            return;
        }

        foreach (var mark in _snap.Marks)
        {
            var screen = camera.WorldToScreenPoint(new Vector3(mark.X, mark.Y + 2.2f, mark.Z));
            if (screen.z <= 0f)
            {
                continue;
            }

            var label = Mathf.RoundToInt(mark.Distance) + "m";
            var rect = new Rect(screen.x - 24f, Screen.height - screen.y - 10f, 48f, 18f);
            GUI.Label(rect, label);
        }
    }

    private static string Key(BepInEx.Configuration.ConfigEntry<KeyCode> entry) => entry.Value.ToString();

    private static string Wrap(string text, int width = 58)
    {
        if (string.IsNullOrEmpty(text) || text.Length <= width)
        {
            return text;
        }

        var words = text.Split(' ');
        var lines = new List<string>();
        var line = "";
        foreach (var word in words)
        {
            var next = line.Length == 0 ? word : line + " " + word;
            if (next.Length > width && line.Length > 0)
            {
                lines.Add(line);
                line = word;
            }
            else
            {
                line = next;
            }
        }

        if (line.Length > 0)
        {
            lines.Add(line);
        }

        return string.Join("\n", lines);
    }
}
