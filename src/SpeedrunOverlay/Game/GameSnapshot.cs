namespace SpeedrunOverlay.Game;

public readonly struct WorldMark
{
    public WorldMark(int prefab, float x, float y, float z, float distance)
    {
        Prefab = prefab;
        X = x;
        Y = y;
        Z = z;
        Distance = distance;
    }

    public int Prefab { get; }
    public float X { get; }
    public float Y { get; }
    public float Z { get; }
    public float Distance { get; }
}

/// <summary>
/// Plain snapshot of the probes that succeeded this refresh.
/// A flag left false means that probe did not confirm, and steps that need it stay on confirm.
/// </summary>
public sealed class GameSnapshot
{
    public bool InWorld { get; set; }
    public bool HasCharacter { get; set; }

    public bool GearLive { get; set; }
    public int GearScore { get; set; }

    public bool VBloodLive { get; set; }
    public HashSet<int> UnlockedVBloods { get; } = new();

    public bool RegionLive { get; set; }
    public int Region { get; set; } = int.MinValue;
    public string RegionName { get; set; } = "";

    public bool HeartLive { get; set; }
    public int HeartLevel { get; set; }

    public bool InventoryLive { get; set; }
    public Dictionary<int, int> Items { get; } = new();

    public bool EquipLive { get; set; }
    public HashSet<int> Equipped { get; } = new();

    public bool PlaceLive { get; set; }
    public Dictionary<int, int> Placed { get; } = new();

    public bool QuestLive { get; set; }
    public HashSet<int> ClaimedQuests { get; } = new();

    public bool PositionLive { get; set; }
    public float X { get; set; }
    public float Y { get; set; }
    public float Z { get; set; }

    public List<WorldMark> Marks { get; } = new();

    public string? GearNote { get; set; }
    public string? VBloodNote { get; set; }
    public string? RegionNote { get; set; }
    public string? HeartNote { get; set; }
    public string? InventoryNote { get; set; }
    public string? EquipNote { get; set; }
    public string? PlaceNote { get; set; }
    public string? QuestNote { get; set; }

    public void Clear()
    {
        InWorld = false;
        HasCharacter = false;
        GearLive = false;
        GearScore = 0;
        VBloodLive = false;
        UnlockedVBloods.Clear();
        RegionLive = false;
        Region = int.MinValue;
        RegionName = "";
        HeartLive = false;
        HeartLevel = 0;
        InventoryLive = false;
        Items.Clear();
        EquipLive = false;
        Equipped.Clear();
        PlaceLive = false;
        Placed.Clear();
        QuestLive = false;
        ClaimedQuests.Clear();
        PositionLive = false;
        Marks.Clear();
        GearNote = VBloodNote = RegionNote = HeartNote = null;
        InventoryNote = EquipNote = PlaceNote = QuestNote = null;
    }
}
