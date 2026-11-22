namespace SpeedrunOverlay;

public readonly struct Pin
{
    public Pin(float x, float y, float z)
    {
        X = x;
        Y = y;
        Z = z;
    }

    public float X { get; }
    public float Y { get; }
    public float Z { get; }
}

/// <summary>
/// One world target per route step, written by the pin bind. There are no map coordinates in the strat.
/// </summary>
public sealed class PinStore
{
    private readonly string _path;
    private readonly Dictionary<int, Pin> _pins = new();

    public PinStore(string path)
    {
        _path = path;
        Load();
    }

    public bool TryGet(int stepNumber, out Pin pin) => _pins.TryGetValue(stepNumber, out pin);

    public void Set(int stepNumber, float x, float y, float z)
    {
        _pins[stepNumber] = new Pin(x, y, z);
        Save();
    }

    private void Load()
    {
        if (!File.Exists(_path))
        {
            return;
        }

        foreach (var raw in File.ReadAllLines(_path))
        {
            var line = raw.Trim();
            if (line.Length == 0 || line.StartsWith('#'))
            {
                continue;
            }

            var split = line.Split('=');
            if (split.Length != 2 || !int.TryParse(split[0], out var step))
            {
                continue;
            }

            var coords = split[1].Split(',');
            if (coords.Length != 3 ||
                !float.TryParse(coords[0], System.Globalization.NumberStyles.Float, System.Globalization.CultureInfo.InvariantCulture, out var x) ||
                !float.TryParse(coords[1], System.Globalization.NumberStyles.Float, System.Globalization.CultureInfo.InvariantCulture, out var y) ||
                !float.TryParse(coords[2], System.Globalization.NumberStyles.Float, System.Globalization.CultureInfo.InvariantCulture, out var z))
            {
                continue;
            }

            _pins[step] = new Pin(x, y, z);
        }
    }

    private void Save()
    {
        var dir = Path.GetDirectoryName(_path);
        if (!string.IsNullOrEmpty(dir))
        {
            Directory.CreateDirectory(dir);
        }

        var lines = new List<string>
        {
            "# step=x,y,z  world position saved by the pin bind",
        };
        foreach (var pair in _pins.OrderBy(pair => pair.Key))
        {
            var pin = pair.Value;
            lines.Add(string.Format(
                System.Globalization.CultureInfo.InvariantCulture,
                "{0}={1},{2},{3}",
                pair.Key,
                pin.X,
                pin.Y,
                pin.Z));
        }

        File.WriteAllLines(_path, lines);
    }
}
