namespace SpeedrunOverlay.Route;

public enum CheckKind
{
    GearScore,
    VBlood,
    Region,
    HeartLevel,
    Item,
    Equipped,
    Placed,
    Quest,
}

/// <summary>
/// One gate that must become true while this step is current.
/// Ids are PrefabGUID hashes, quest hashes, or WorldRegionType values.
/// </summary>
public sealed class RouteCheck
{
    public RouteCheck(CheckKind kind, int amount, int[] ids, string label, bool matchAll = false)
    {
        Kind = kind;
        Amount = amount;
        Ids = ids;
        Label = label;
        MatchAll = matchAll;
    }

    public CheckKind Kind { get; }
    public int Amount { get; }
    public int[] Ids { get; }
    public string Label { get; }
    public bool MatchAll { get; }
}

public sealed class RouteStep
{
    public RouteStep(
        int number,
        string text,
        string hint,
        string completesWhen,
        RouteCheck[] checks,
        int[] highlights,
        bool conditional = false)
    {
        Number = number;
        Text = text;
        Hint = hint;
        CompletesWhen = completesWhen;
        Checks = checks;
        Highlights = highlights;
        Conditional = conditional;
    }

    public int Number { get; }
    public string Text { get; }
    public string Hint { get; }
    public string CompletesWhen { get; }
    public RouteCheck[] Checks { get; }
    public int[] Highlights { get; }

    /// <summary>
    /// Conditional route lines ("if you see it", "if you have them", "if you can") never auto-advance.
    /// </summary>
    public bool Conditional { get; }

    public bool CanAutoComplete => !Conditional && Checks.Length > 0;
}
