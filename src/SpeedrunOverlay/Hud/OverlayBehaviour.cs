using SpeedrunOverlay.Game;
using SpeedrunOverlay.Route;
using UnityEngine;
using UnityEngine.UI;

namespace SpeedrunOverlay.Hud;

/// <summary>
/// Step list on a DontDestroyOnLoad screen-space canvas, refreshed from Update.
/// OnGUI is not used. Unhollowed MonoBehaviour does not declare it, and Il2CppInterop
/// does not register that message, so a loaded plugin can draw nothing. GUILayout can
/// also throw before any pixels.
/// </summary>
public sealed class OverlayBehaviour : MonoBehaviour
{
    private const int MarkLabelCount = 24;
    private const int MaxCreateAttempts = 8;

    private OverlayConfig? _config;
    private RouteRunner? _runner;
    private PinStore? _pins;
    private GameReader? _reader;
    private WorldMarkers? _markers;
    private readonly GameSnapshot _snap = new();
    private float _nextRead;

    private Font? _font;
    private string _fontSource = "none";
    private GameObject? _root;
    private Canvas? _canvas;
    private GameObject? _panel;
    private RectTransform? _panelRect;
    private Text? _body;
    private string? _shown;
    private Texture2D? _pixel;
    private readonly Text?[] _marks = new Text?[MarkLabelCount];
    private Text? _bloodLabel;
    private Text? _bloodArrow;
    private Text? _bloodMeters;
    private bool _loggedDrawError;
    private bool _loggedTickError;
    private int _createAttempts;
    private float _panelRetryAt;
    private bool _gaveUp;

    public OverlayBehaviour(IntPtr ptr) : base(ptr)
    {
    }

    public void Bind(OverlayConfig config, RouteRunner runner, PinStore pins)
    {
        _config = config;
        _runner = runner;
        _pins = pins;
        _reader = new GameReader();
        _markers = new WorldMarkers();
        EnsurePanel();
        RefreshHud();
    }

    public void OnDestroy()
    {
        _markers?.Destroy();
        if (_root != null)
        {
            Destroy(_root);
            _root = null;
        }

        if (_pixel != null)
        {
            Destroy(_pixel);
            _pixel = null;
        }
    }

    public void Update()
    {
        if (_config == null || _runner == null || _pins == null || _reader == null || _markers == null)
        {
            return;
        }

        try
        {
            TickInputAndWorld();
        }
        catch (Exception ex)
        {
            if (!_loggedTickError)
            {
                _loggedTickError = true;
                Plugin.Log?.LogError("Speedrun overlay update failed: " + ex);
            }
        }

        EnsurePanel();
        RefreshHud();
    }

    private void TickInputAndWorld()
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

    private void EnsurePanel()
    {
        if (_gaveUp || _root != null)
        {
            return;
        }

        if (_createAttempts > 0 && Time.unscaledTime < _panelRetryAt)
        {
            return;
        }

        _createAttempts++;
        try
        {
            BuildPanel(_createAttempts == 1);
            Plugin.Log?.LogInfo("Speedrun overlay panel created. Font: " + _fontSource + ".");
        }
        catch (Exception ex)
        {
            DestroyPartialPanel();
            _panelRetryAt = Time.unscaledTime + 0.5f;
            if (_createAttempts >= MaxCreateAttempts)
            {
                _gaveUp = true;
            }

            if (!_loggedDrawError)
            {
                _loggedDrawError = true;
                Plugin.Log?.LogError("Speedrun overlay draw failed: " + ex);
            }
        }
    }

    private void DestroyPartialPanel()
    {
        if (_root != null)
        {
            Destroy(_root);
            _root = null;
        }

        if (_pixel != null)
        {
            Destroy(_pixel);
            _pixel = null;
        }

        _canvas = null;
        _panel = null;
        _panelRect = null;
        _body = null;
        _shown = null;
        _bloodLabel = null;
        _bloodArrow = null;
        _bloodMeters = null;
        Array.Clear(_marks, 0, _marks.Length);
    }

    private void BuildPanel(bool logFontMisses)
    {
        _font = LoadFont(logFontMisses, out _fontSource);

        _root = new GameObject("SpeedrunOverlayCanvas");
        _root.hideFlags = HideFlags.HideAndDontSave;
        DontDestroyOnLoad(_root);

        _canvas = _root.AddComponent<Canvas>();
        _canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        _canvas.overrideSorting = true;
        _canvas.sortingOrder = 5000;
        _canvas.pixelPerfect = false;

        var scaler = _root.AddComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ConstantPixelSize;
        scaler.scaleFactor = 1f;

        var panelGo = new GameObject("Panel");
        panelGo.transform.SetParent(_root.transform, false);
        var background = panelGo.AddComponent<RawImage>();
        _pixel = new Texture2D(1, 1, TextureFormat.RGBA32, false);
        _pixel.hideFlags = HideFlags.HideAndDontSave;
        _pixel.SetPixel(0, 0, Color.white);
        _pixel.Apply();
        background.texture = _pixel;
        background.color = new Color(0.02f, 0.02f, 0.04f, 0.84f);
        background.raycastTarget = false;
        _panel = panelGo;
        _panelRect = background.rectTransform;
        _panelRect.anchorMin = new Vector2(0f, 1f);
        _panelRect.anchorMax = new Vector2(0f, 1f);
        _panelRect.pivot = new Vector2(0f, 1f);
        _panelRect.anchoredPosition = new Vector2(16f, -16f);
        _panelRect.sizeDelta = new Vector2(540f, 420f);

        var bodyGo = new GameObject("Body");
        bodyGo.transform.SetParent(panelGo.transform, false);
        _body = bodyGo.AddComponent<Text>();
        var bodyRect = _body.rectTransform;
        bodyRect.anchorMin = new Vector2(0f, 0f);
        bodyRect.anchorMax = new Vector2(1f, 1f);
        bodyRect.offsetMin = new Vector2(14f, 12f);
        bodyRect.offsetMax = new Vector2(-14f, -12f);
        StyleText(_body, 16, TextAnchor.UpperLeft, Color.white, HorizontalWrapMode.Wrap);
        AddShadow(bodyGo);

        var labelsGo = new GameObject("Labels");
        var labels = labelsGo.AddComponent<RectTransform>();
        labels.SetParent(_root.transform, false);
        labels.anchorMin = new Vector2(0f, 0f);
        labels.anchorMax = new Vector2(1f, 1f);
        labels.offsetMin = new Vector2(0f, 0f);
        labels.offsetMax = new Vector2(0f, 0f);

        var markColor = new Color(0.55f, 0.95f, 1f, 1f);
        for (var i = 0; i < _marks.Length; i++)
        {
            _marks[i] = CreateScreenText(labels, "Mark" + i, 15, markColor, new Vector2(0.5f, 1f), new Vector2(72f, 20f));
        }

        var blood = new Color(1f, 0.25f, 0.18f, 1f);
        _bloodLabel = CreateScreenText(labels, "BloodLabel", 16, blood, new Vector2(0.5f, 1f), new Vector2(240f, 22f));
        _bloodArrow = CreateScreenText(labels, "BloodArrow", 22, blood, new Vector2(0.5f, 0.5f), new Vector2(28f, 28f));
        _bloodArrow.text = ">";
        _bloodArrow.alignment = TextAnchor.MiddleCenter;
        _bloodMeters = CreateScreenText(labels, "BloodMeters", 15, blood, new Vector2(0.5f, 1f), new Vector2(80f, 20f));
    }

    private Text CreateScreenText(Transform parent, string name, int fontSize, Color color, Vector2 pivot, Vector2 size)
    {
        var go = new GameObject(name);
        go.transform.SetParent(parent, false);
        var text = go.AddComponent<Text>();
        var rect = text.rectTransform;
        rect.anchorMin = new Vector2(0f, 0f);
        rect.anchorMax = new Vector2(0f, 0f);
        rect.pivot = pivot;
        rect.sizeDelta = size;
        StyleText(text, fontSize, TextAnchor.MiddleCenter, color, HorizontalWrapMode.Overflow);
        AddShadow(go);
        go.SetActive(false);
        return text;
    }

    private void StyleText(Text text, int size, TextAnchor align, Color color, HorizontalWrapMode wrap)
    {
        if (_font != null)
        {
            text.font = _font;
        }

        text.fontSize = size;
        text.alignment = align;
        text.color = color;
        text.horizontalOverflow = wrap;
        text.verticalOverflow = VerticalWrapMode.Overflow;
        text.raycastTarget = false;
        text.supportRichText = false;
    }

    private static void AddShadow(GameObject go)
    {
        var shadow = go.AddComponent<Shadow>();
        shadow.effectColor = new Color(0f, 0f, 0f, 0.9f);
        shadow.effectDistance = new Vector2(1f, -1f);
    }

    private static Font? LoadFont(bool logMisses, out string source)
    {
        var builtin = BuiltinFont("LegacyRuntime.ttf", logMisses);
        if (builtin != null)
        {
            source = "LegacyRuntime.ttf";
            return builtin;
        }

        builtin = BuiltinFont("Arial.ttf", logMisses);
        if (builtin != null)
        {
            source = "Arial.ttf";
            return builtin;
        }

        try
        {
            var fallback = Font.GetDefault();
            if (fallback != null)
            {
                source = "Font.GetDefault";
                return fallback;
            }
        }
        catch (Exception ex)
        {
            if (logMisses)
            {
                Plugin.Log?.LogWarning("Speedrun overlay font Font.GetDefault failed: " + ex.Message);
            }
        }

        foreach (var name in new[] { "Arial", "Segoe UI" })
        {
            try
            {
                var os = Font.CreateDynamicFontFromOSFont(name, 16);
                if (os != null)
                {
                    source = name;
                    return os;
                }
            }
            catch (Exception ex)
            {
                if (logMisses)
                {
                    Plugin.Log?.LogWarning("Speedrun overlay font " + name + " failed: " + ex.Message);
                }
            }
        }

        source = "none";
        return null;
    }

    private static Font? BuiltinFont(string path, bool logMisses)
    {
        try
        {
            var font = Resources.GetBuiltinResource<Font>(path);
            return font != null ? font : null;
        }
        catch (Exception ex)
        {
            if (logMisses)
            {
                Plugin.Log?.LogWarning("Speedrun overlay font " + path + " failed: " + ex.Message);
            }

            return null;
        }
    }

    private void RefreshHud()
    {
        if (_root == null || _config == null || _runner == null || _pins == null || _canvas == null)
        {
            return;
        }

        try
        {
            DrawHud();
        }
        catch (Exception ex)
        {
            if (!_loggedDrawError)
            {
                _loggedDrawError = true;
                Plugin.Log?.LogError("Speedrun overlay draw failed: " + ex);
            }
        }
    }

    private void DrawHud()
    {
        if (_config == null || _runner == null || _pins == null || _panel == null || _body == null || _panelRect == null || _canvas == null)
        {
            return;
        }

        var show = _config.ShowOverlay.Value;
        if (_panel.activeSelf != show)
        {
            _panel.SetActive(show);
        }

        if (show)
        {
            var body = BuildBody();
            if (!string.Equals(_shown, body, StringComparison.Ordinal))
            {
                _shown = body;
                _body.text = body;
                _panelRect.sizeDelta = new Vector2(540f, PanelHeight(body));
            }
        }

        var scale = _canvas.scaleFactor;
        if (scale < 0.01f)
        {
            scale = 1f;
        }

        DrawScreenMarks(scale);
        DrawBloodPointer(scale);
    }

    private string BuildBody()
    {
        if (_config == null || _runner == null || _pins == null)
        {
            return "";
        }

        var step = _runner.Current;
        var pinned = _pins.TryGet(step.Number, out _);
        var pathLine = !_config.ShowPath.Value
            ? "Path hidden (" + Key(_config.TogglePath) + ")."
            : pinned
                ? "Path pinned for this step."
                : "Path off until you pin this step (" + Key(_config.PinTarget) + "). No coordinates are shipped with the route.";

        var lines = new List<string>
        {
            Wrap("Speedrun  " + step.Number + " / " + _runner.Count),
            Wrap(step.Text),
        };
        if (!string.IsNullOrWhiteSpace(step.Hint))
        {
            lines.Add(Wrap("Hint: " + step.Hint));
        }

        lines.Add(Wrap("Completes when: " + step.CompletesWhen));
        lines.Add(Wrap(_runner.GateStatus(_snap)));
        lines.Add(Wrap(pathLine));
        lines.Add(Wrap(BloodLine()));
        if (_config.ShowHighlights.Value && step.Highlights.Length > 0)
        {
            lines.Add(
                _snap.Marks.Count > 0
                    ? _snap.Marks.Count + " highlighted nearby."
                    : "No matching nodes or bosses are loaded nearby.");
        }

        lines.Add(Wrap(
            Key(_config.Confirm) + " confirm   " +
            Key(_config.Back) + " back   " +
            Key(_config.ToggleOverlay) + " overlay   " +
            Key(_config.TogglePath) + " path   " +
            Key(_config.ToggleHighlights) + " highlights   " +
            Key(_config.PinTarget) + " pin   " +
            Key(_config.BloodPrevious) + "/" + Key(_config.BloodNext) + " blood"));
        return string.Join("\n", lines);
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

    private void DrawBloodPointer(float scale)
    {
        if (_bloodLabel == null || _bloodArrow == null || _bloodMeters == null)
        {
            return;
        }

        if (_config == null || !_snap.BloodTargetFound)
        {
            Hide(_bloodLabel);
            Hide(_bloodArrow);
            Hide(_bloodMeters);
            return;
        }

        var blood = BloodTypes.Find(_config.BloodType.Value);
        if (blood.IsOff || _snap.BloodTypeGuid != blood.Guid)
        {
            Hide(_bloodLabel);
            Hide(_bloodArrow);
            Hide(_bloodMeters);
            return;
        }

        var camera = Camera.main;
        if (camera == null)
        {
            Hide(_bloodLabel);
            Hide(_bloodArrow);
            Hide(_bloodMeters);
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
        var meters = Mathf.RoundToInt(_snap.BloodDistance) + "m";
        if (onScreen)
        {
            Hide(_bloodArrow);
            Hide(_bloodMeters);
            _bloodLabel.text = "100% " + blood.Name + "  " + meters;
            Place(_bloodLabel, screen.x, screen.y + 28f, scale, 0f);
            return;
        }

        Hide(_bloodLabel);
        var margin = 40f;
        var center = new Vector2(Screen.width * 0.5f, Screen.height * 0.5f);
        var delta = new Vector2(screen.x, screen.y) - center;
        var half = new Vector2(Screen.width * 0.5f - margin, Screen.height * 0.5f - margin);
        var fit = Mathf.Max(Mathf.Abs(delta.x) / half.x, Mathf.Abs(delta.y) / half.y);
        if (fit < 0.001f)
        {
            fit = 1f;
        }

        var edge = center + delta / fit;
        var angle = Mathf.Atan2(delta.y, delta.x) * Mathf.Rad2Deg;
        Place(_bloodArrow, edge.x, edge.y, scale, angle);
        _bloodMeters.text = meters;
        Place(_bloodMeters, edge.x, edge.y - 10f, scale, 0f);
    }

    private void DrawScreenMarks(float scale)
    {
        var show = _config != null && _config.ShowHighlights.Value && _snap.Marks.Count > 0 && Camera.main != null;
        var camera = show ? Camera.main : null;
        for (var i = 0; i < _marks.Length; i++)
        {
            var label = _marks[i];
            if (label == null)
            {
                continue;
            }

            if (camera == null || i >= _snap.Marks.Count)
            {
                Hide(label);
                continue;
            }

            var mark = _snap.Marks[i];
            var screen = camera.WorldToScreenPoint(new Vector3(mark.X, mark.Y + 2.2f, mark.Z));
            if (screen.z <= 0f)
            {
                Hide(label);
                continue;
            }

            label.text = Mathf.RoundToInt(mark.Distance) + "m";
            Place(label, screen.x, screen.y + 10f, scale, 0f);
        }
    }

    private static void Place(Text label, float x, float y, float scale, float angle)
    {
        var go = label.gameObject;
        if (!go.activeSelf)
        {
            go.SetActive(true);
        }

        var rect = label.rectTransform;
        rect.anchoredPosition = new Vector2(x / scale, y / scale);
        rect.localEulerAngles = new Vector3(0f, 0f, angle);
    }

    private static void Hide(Text label)
    {
        if (label.gameObject.activeSelf)
        {
            label.gameObject.SetActive(false);
        }
    }

    private static string Key(BepInEx.Configuration.ConfigEntry<KeyCode> entry) => entry.Value.ToString();

    private static float PanelHeight(string text)
    {
        var lines = 1;
        var column = 0;
        for (var i = 0; i < text.Length; i++)
        {
            if (text[i] == '\n')
            {
                lines++;
                column = 0;
                continue;
            }

            column++;
            if (column >= 58)
            {
                lines++;
                column = 0;
            }
        }

        return Mathf.Clamp(28f + lines * 22f, 120f, 680f);
    }

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
