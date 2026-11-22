namespace SpeedrunOverlay.Game;

/// <summary>
/// Drinkable blood types. Hashes match the published BloodType_* prefab table
/// (vrisingmods Blood prefabs and the RisingV BloodType enum).
/// </summary>
public sealed class BloodTypeOption
{
    public BloodTypeOption(string name, int guid)
    {
        Name = name;
        Guid = guid;
    }

    public string Name { get; }

    /// <summary>PrefabGUID hash. Zero means the hunt is off.</summary>
    public int Guid { get; }

    public bool IsOff => Guid == 0;
}

public static class BloodTypes
{
    public const float PerfectQuality = 99.5f;

    public static readonly BloodTypeOption Off = new("Off", 0);

    public static readonly BloodTypeOption[] Options =
    {
        Off,
        new("Worker", -1776904174),
        new("Warrior", -516976528),
        new("Rogue", -1620185637),
        new("Scholar", 1476452791),
        new("Brute", 804798592),
        new("Creature", 524822543),
        new("Mutant", 1821108694),
        new("Draculin", 1328126535),
        new("Dracula", 2010023718),
    };

    public static BloodTypeOption Find(string? name)
    {
        if (string.IsNullOrWhiteSpace(name))
        {
            return Off;
        }

        foreach (var option in Options)
        {
            if (string.Equals(option.Name, name, StringComparison.OrdinalIgnoreCase))
            {
                return option;
            }
        }

        return Off;
    }

    public static BloodTypeOption Step(string? name, int direction)
    {
        var current = Find(name);
        var index = 0;
        for (var i = 0; i < Options.Length; i++)
        {
            if (Options[i].Guid == current.Guid && Options[i].Name == current.Name)
            {
                index = i;
                break;
            }
        }

        var next = index + direction;
        if (next < 0)
        {
            next = Options.Length - 1;
        }
        else if (next >= Options.Length)
        {
            next = 0;
        }

        return Options[next];
    }
}
