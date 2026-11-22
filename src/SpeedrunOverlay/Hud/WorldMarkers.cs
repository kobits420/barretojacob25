using SpeedrunOverlay.Game;
using UnityEngine;

namespace SpeedrunOverlay.Hud;

/// <summary>
/// Client-only line from the player to the pinned step target, plus short beams on highlighted entities.
/// </summary>
public sealed class WorldMarkers
{
    private const int BeamCount = 24;
    private LineRenderer? _path;
    private readonly LineRenderer[] _beams = new LineRenderer[BeamCount];
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
