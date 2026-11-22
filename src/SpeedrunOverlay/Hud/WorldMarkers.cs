using SpeedrunOverlay.Game;
using UnityEngine;

namespace SpeedrunOverlay.Hud;

/// <summary>
/// Client-only line from the player to the pinned step target, plus short beams on highlighted entities.
/// </summary>
public sealed class WorldMarkers
{
    private const int BeamCount = 24;
    private const int BloodLineCount = 6;
    private LineRenderer? _path;
    private readonly LineRenderer[] _beams = new LineRenderer[BeamCount];
    private readonly LineRenderer?[] _blood = new LineRenderer?[BloodLineCount];
    private Material? _material;

    public void DrawPath(bool enabled, bool hasPlayer, float px, float py, float pz, bool hasPin, Pin pin)
    {
        var line = PathLine();
        if (!enabled || !hasPlayer || !hasPin)
        {
            line.enabled = false;
            return;
        }

        line.enabled = true;
        line.positionCount = 2;
        line.SetPosition(0, new Vector3(px, py + 1.2f, pz));
        line.SetPosition(1, new Vector3(pin.X, pin.Y + 1.2f, pin.Z));
    }

    public void DrawHighlights(bool enabled, IReadOnlyList<WorldMark> marks)
    {
        for (var i = 0; i < _beams.Length; i++)
        {
            var show = enabled && i < marks.Count;
            var beam = Beam(i);
            beam.enabled = show;
            if (!show)
            {
                continue;
            }

            var mark = marks[i];
            beam.SetPosition(0, new Vector3(mark.X, mark.Y + 0.2f, mark.Z));
            beam.SetPosition(1, new Vector3(mark.X, mark.Y + 4.5f, mark.Z));
        }
    }

    public void DrawBloodArrow(bool enabled, bool hasPlayer, float px, float py, float pz, bool hasTarget, float x, float y, float z)
    {
        if (!enabled || !hasPlayer || !hasTarget)
        {
            SetBlood(false);
            return;
        }

        var player = new Vector3(px, py + 1.15f, pz);
        var target = new Vector3(x, y + 1.15f, z);
        var flat = target - player;
        flat.y = 0f;
        if (flat.sqrMagnitude < 0.36f)
        {
            SetBlood(false);
            return;
        }

        var dir = flat.normalized;
        var side = Vector3.Cross(dir, Vector3.up);
        var tip = target + dir * 0.15f;
        var neck = target - dir * 1.7f;
        PlaceBlood(0, player, target, 0.16f);
        PlaceBlood(1, tip, neck + side * 0.75f, 0.22f);
        PlaceBlood(2, tip, neck - side * 0.75f, 0.22f);

        var localStart = player + dir * 1.5f;
        var localTip = player + dir * 8f;
        var localNeck = localTip - dir * 1.4f;
        PlaceBlood(3, localStart, localTip, 0.28f);
        PlaceBlood(4, localTip, localNeck + side * 0.7f, 0.28f);
        PlaceBlood(5, localTip, localNeck - side * 0.7f, 0.28f);
    }

    public void Destroy()
    {
        if (_path != null)
        {
            UnityEngine.Object.Destroy(_path.gameObject);
            _path = null;
        }

        foreach (var beam in _beams)
        {
            if (beam != null)
            {
                UnityEngine.Object.Destroy(beam.gameObject);
            }
        }

        for (var i = 0; i < _blood.Length; i++)
        {
            if (_blood[i] != null)
            {
                UnityEngine.Object.Destroy(_blood[i]!.gameObject);
                _blood[i] = null;
            }
        }

        if (_material != null)
        {
            UnityEngine.Object.Destroy(_material);
            _material = null;
        }
    }

    private LineRenderer PathLine()
    {
        if (_path != null)
        {
            return _path;
        }

        _path = CreateLine("SpeedrunPath", new Color(1f, 0.82f, 0.25f, 0.95f), 0.18f);
        return _path;
    }

    private LineRenderer Beam(int index)
    {
        if (_beams[index] != null)
        {
            return _beams[index];
        }

        _beams[index] = CreateLine("SpeedrunHighlight" + index, new Color(0.35f, 0.95f, 1f, 0.9f), 0.12f);
        _beams[index].enabled = false;
        return _beams[index];
    }

    private void SetBlood(bool enabled)
    {
        foreach (var line in _blood)
        {
            if (line != null)
            {
                line.enabled = enabled;
            }
        }
    }

    private void PlaceBlood(int index, Vector3 from, Vector3 to, float width)
    {
        var line = BloodLine(index);
        line.enabled = true;
        line.startWidth = width;
        line.endWidth = width * 0.35f;
        line.positionCount = 2;
        line.SetPosition(0, from);
        line.SetPosition(1, to);
    }

    private LineRenderer BloodLine(int index)
    {
        if (_blood[index] != null)
        {
            return _blood[index]!;
        }

        var line = CreateLine("SpeedrunBloodArrow" + index, new Color(0.95f, 0.12f, 0.1f, 1f), 0.2f);
        line.enabled = false;
        _blood[index] = line;
        return line;
    }

    private LineRenderer CreateLine(string name, Color color, float width)
    {
        var go = new GameObject(name);
        UnityEngine.Object.DontDestroyOnLoad(go);
        var line = go.AddComponent<LineRenderer>();
        var material = Material();
        if (material != null)
        {
            line.material = material;
        }
        line.startColor = color;
        line.endColor = color;
        line.startWidth = width;
        line.endWidth = width * 0.4f;
        line.useWorldSpace = true;
        line.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
        line.receiveShadows = false;
        line.positionCount = 2;
        line.enabled = false;
        return line;
    }

    private Material? Material()
    {
        if (_material != null)
        {
            return _material;
        }

        var shader = Shader.Find("Sprites/Default")
            ?? Shader.Find("Hidden/Internal-Colored")
            ?? Shader.Find("UI/Default")
            ?? Shader.Find("Standard");
        _material = shader != null ? new Material(shader) : null;
        return _material;
    }
}
