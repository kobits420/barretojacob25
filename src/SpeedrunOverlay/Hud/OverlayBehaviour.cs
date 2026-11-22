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

        if (OverlayConfig.Pressed(_config.BloodPrevious.Value))
        {
            _config.BloodType.Value = BloodTypes.Step(_config.BloodType.Value, -1).Name;
            _snap.BloodTargetFound = false;
        }

        if (OverlayConfig.Pressed(_config.BloodNext.Value))
        {
            _config.BloodType.Value = BloodTypes.Step(_config.BloodType.Value, 1).Name;
            _snap.BloodTargetFound = false;
        }

        var blood = BloodTypes.Find(_config.BloodType.Value);
        if (Time.unscaledTime >= _nextRead)
        {
            _nextRead = Time.unscaledTime + 0.35f;
            var highlights = _config.ShowHighlights.Value ? _runner.Current.Highlights : null;
            _reader.Refresh(_snap, highlights, blood.Guid);
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
        var showBlood = !blood.IsOff && _snap.BloodTargetFound && _snap.BloodTypeGuid == blood.Guid;
        _markers.DrawBloodArrow(
            showBlood,
            _snap.PositionLive,
            _snap.X,
            _snap.Y,
            _snap.Z,
            showBlood,
            _snap.BloodX,
            _snap.BloodY,
            _snap.BloodZ);
    }

    public void OnGUI()
    {
        if (_config == null || _runner == null || _pins == null || !_config.ShowOverlay.Value)
        {
            DrawScreenMarks();
            DrawBloodPointer();
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
        var area = new Rect(16f, 16f, 520f, 460f);
        GUI.Box(area, GUIContent.none);
        GUILayout.BeginArea(new Rect(28f, 24f, 496f, 440f));
        GUILayout.Label(Wrap($"Speedrun  {step.Number} / {_runner.Count}"));
        GUILayout.Label(Wrap(step.Text));
        if (!string.IsNullOrWhiteSpace(step.Hint))
        {
            GUILayout.Label(Wrap("Hint: " + step.Hint));
        }

        GUILayout.Label(Wrap("Completes when: " + step.CompletesWhen));
        GUILayout.Label(Wrap(_runner.GateStatus(_snap)));
        GUILayout.Label(Wrap(pathLine));
        GUILayout.Label(Wrap(BloodLine()));
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
            Key(_config.PinTarget) + " pin   " +
            Key(_config.BloodPrevious) + "/" + Key(_config.BloodNext) + " blood");
        GUILayout.EndArea();
        DrawScreenMarks();
        DrawBloodPointer();
    }

    private string BloodLine()
    {
        if (_config == null)
        {
            return "";
        }

        var blood = BloodTypes.Find(_config.BloodType.Value);
        if (blood.IsOff)
        {
            return "Blood hunt off (" + Key(_config.BloodPrevious) + "/" + Key(_config.BloodNext) + " to choose a type).";
        }

        if (!_snap.BloodSearchLive)
        {
            return "Blood hunt: " + blood.Name + ". Quality could not be read" +
                (string.IsNullOrEmpty(_snap.BloodNote) ? "." : " (" + _snap.BloodNote + ").");
        }

        if (!_snap.BloodTargetFound || _snap.BloodTypeGuid != blood.Guid)
        {
            return "Blood hunt: " + blood.Name + ". No 100% is loaded nearby.";
        }

        return "Blood hunt: " + blood.Name + " " + Mathf.RoundToInt(_snap.BloodQuality) +
            "% — " + Mathf.RoundToInt(_snap.BloodDistance) + "m. Red arrow.";
    }

    private void DrawBloodPointer()
    {
        if (_config == null || !_snap.BloodTargetFound)
        {
            return;
        }

        var blood = BloodTypes.Find(_config.BloodType.Value);
        if (blood.IsOff || _snap.BloodTypeGuid != blood.Guid)
        {
            return;
        }

        var camera = Camera.main;
        if (camera == null)
        {
            return;
        }

        var world = new Vector3(_snap.BloodX, _snap.BloodY + 2.2f, _snap.BloodZ);
        var screen = camera.WorldToScreenPoint(world);
        var behind = screen.z <= 0f;
        if (behind)
        {
            screen.x = Screen.width - screen.x;
            screen.y = Screen.height - screen.y;
        }

        var onScreen = !behind &&
            screen.x >= 24f && screen.x <= Screen.width - 24f &&
            screen.y >= 24f && screen.y <= Screen.height - 24f;
        var previous = GUI.color;
        GUI.color = new Color(1f, 0.25f, 0.18f);
        var meters = Mathf.RoundToInt(_snap.BloodDistance) + "m";
        if (onScreen)
        {
            GUI.Label(
                new Rect(screen.x - 80f, Screen.height - screen.y - 28f, 160f, 20f),
                "100% " + blood.Name + "  " + meters);
        }
        else
        {
            var margin = 40f;
            var center = new Vector2(Screen.width * 0.5f, Screen.height * 0.5f);
            var gui = new Vector2(screen.x, Screen.height - screen.y);
            var delta = gui - center;
            var half = new Vector2(Screen.width * 0.5f - margin, Screen.height * 0.5f - margin);
            var scale = Mathf.Max(Mathf.Abs(delta.x) / half.x, Mathf.Abs(delta.y) / half.y);
            if (scale < 0.001f)
            {
                scale = 1f;
            }

            var edge = center + delta / scale;
            var angle = Mathf.Atan2(delta.y, delta.x) * Mathf.Rad2Deg;
            var matrix = GUI.matrix;
            GUIUtility.RotateAroundPivot(angle, edge);
            GUI.Label(new Rect(edge.x - 10f, edge.y - 10f, 24f, 20f), ">");
            GUI.matrix = matrix;
            GUI.Label(new Rect(edge.x - 36f, edge.y + 10f, 72f, 18f), meters);
        }

        GUI.color = previous;
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
