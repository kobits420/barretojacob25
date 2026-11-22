using SpeedrunOverlay.Game;
using SpeedrunOverlay.Route;

namespace SpeedrunOverlay.Verify;

/// <summary>
/// A step gate. Stub verifiers never report satisfied, so those steps wait for confirm.
/// </summary>
public interface IStepVerifier
{
    string Name { get; }
    bool IsStub { get; }
    string StubReason { get; }
    bool IsLive(GameSnapshot snap);
    bool Satisfied(GameSnapshot snap, RouteCheck check);
    string Status(GameSnapshot snap, RouteCheck check);
}

public static class VerifierSet
{
    public static readonly IStepVerifier[] All =
    {
        new GearScoreVerifier(),
        new VBloodUnlockVerifier(),
        new RegionVerifier(),
        new CastleHeartVerifier(),
        new InventoryVerifier(),
        new EquippedVerifier(),
        new PlacementVerifier(),
        new QuestVerifier(),
        new KillEventVerifier(),
        new SpellUnlockVerifier(),
    };

    public static IStepVerifier For(CheckKind kind)
    {
        foreach (var verifier in All)
        {
            if (verifier is IKindVerifier typed && typed.Kind == kind)
            {
                return verifier;
            }
        }

        throw new InvalidOperationException("No verifier for " + kind);
    }
}

internal interface IKindVerifier
{
    CheckKind Kind { get; }
}

public sealed class GearScoreVerifier : IStepVerifier, IKindVerifier
{
    public CheckKind Kind => CheckKind.GearScore;
    public string Name => "Gear score";
    public bool IsStub => false;
    public string StubReason => "";
    public bool IsLive(GameSnapshot snap) => snap.GearLive;

    public bool Satisfied(GameSnapshot snap, RouteCheck check) => snap.GearScore >= check.Amount;

    public string Status(GameSnapshot snap, RouteCheck check) =>
        snap.GearLive ? $"Gear score {snap.GearScore} / {check.Amount}" : "Gear score unread (" + snap.GearNote + ")";
}

public sealed class VBloodUnlockVerifier : IStepVerifier, IKindVerifier
{
    public CheckKind Kind => CheckKind.VBlood;
    public string Name => "V Blood unlock";
    public bool IsStub => false;
    public string StubReason => "";
    public bool IsLive(GameSnapshot snap) => snap.VBloodLive;

    public bool Satisfied(GameSnapshot snap, RouteCheck check)
    {
        foreach (var id in check.Ids)
        {
            if (!snap.UnlockedVBloods.Contains(id))
            {
                return false;
            }
        }

        return check.Ids.Length > 0;
    }

    public string Status(GameSnapshot snap, RouteCheck check)
    {
        if (!snap.VBloodLive)
        {
            return "V Blood unlock unread (" + snap.VBloodNote + ")";
        }

        var unlocked = check.Ids.Length > 0 && snap.UnlockedVBloods.Contains(check.Ids[0]);
        return unlocked ? check.Label + " is unlocked" : check.Label + " is not unlocked yet";
    }
}

public sealed class RegionVerifier : IStepVerifier, IKindVerifier
{
    public CheckKind Kind => CheckKind.Region;
    public string Name => "World region";
    public bool IsStub => false;
    public string StubReason => "";
    public bool IsLive(GameSnapshot snap) => snap.RegionLive;

    public bool Satisfied(GameSnapshot snap, RouteCheck check)
    {
        foreach (var id in check.Ids)
        {
            if (snap.Region == id)
            {
                return true;
            }
        }

        return false;
    }

    public string Status(GameSnapshot snap, RouteCheck check)
    {
        if (!snap.RegionLive)
        {
            return "Region unread (" + snap.RegionNote + ")";
        }

        return Satisfied(snap, check)
            ? "In " + check.Label
            : "Region now: " + snap.RegionName;
    }
}

public sealed class CastleHeartVerifier : IStepVerifier, IKindVerifier
{
    public CheckKind Kind => CheckKind.HeartLevel;
    public string Name => "Castle Heart level";
    public bool IsStub => false;
    public string StubReason => "";
    public bool IsLive(GameSnapshot snap) => snap.HeartLive;

    public bool Satisfied(GameSnapshot snap, RouteCheck check) => snap.HeartLevel >= check.Amount;

    public string Status(GameSnapshot snap, RouteCheck check) =>
        snap.HeartLive
            ? $"Castle Heart level {snap.HeartLevel} / {check.Amount}"
            : "Castle Heart unread (" + snap.HeartNote + ")";
}

public sealed class InventoryVerifier : IStepVerifier, IKindVerifier
{
    public CheckKind Kind => CheckKind.Item;
    public string Name => "Inventory";
    public bool IsStub => false;
    public string StubReason => "";
    public bool IsLive(GameSnapshot snap) => snap.InventoryLive;

    public bool Satisfied(GameSnapshot snap, RouteCheck check) => Count(snap, check) >= check.Amount;

    public string Status(GameSnapshot snap, RouteCheck check)
    {
        if (!snap.InventoryLive)
        {
            return check.Label + " unread (" + snap.InventoryNote + ")";
        }

        return check.Label + " x" + Count(snap, check);
    }

    private static int Count(GameSnapshot snap, RouteCheck check)
    {
        if (check.MatchAll)
        {
            var least = int.MaxValue;
            foreach (var id in check.Ids)
            {
                snap.Items.TryGetValue(id, out var have);
                if (have < least)
                {
                    least = have;
                }
            }

            return least == int.MaxValue ? 0 : least;
        }

        var sum = 0;
        foreach (var id in check.Ids)
        {
            snap.Items.TryGetValue(id, out var have);
            if (snap.Equipped.Contains(id))
            {
                have += 1;
            }

            sum += have;
        }

        return sum;
    }
}

public sealed class EquippedVerifier : IStepVerifier, IKindVerifier
{
    public CheckKind Kind => CheckKind.Equipped;
    public string Name => "Equipped item";
    public bool IsStub => false;
    public string StubReason => "";
    public bool IsLive(GameSnapshot snap) => snap.EquipLive;

    public bool Satisfied(GameSnapshot snap, RouteCheck check)
    {
        foreach (var id in check.Ids)
        {
            if (snap.Equipped.Contains(id))
            {
                return true;
            }
        }

        return false;
    }

    public string Status(GameSnapshot snap, RouteCheck check)
    {
        if (!snap.EquipLive)
        {
            return check.Label + " unread (" + snap.EquipNote + ")";
        }

        return Satisfied(snap, check) ? check.Label + " equipped" : check.Label + " not equipped";
    }
}

public sealed class PlacementVerifier : IStepVerifier, IKindVerifier
{
    public CheckKind Kind => CheckKind.Placed;
    public string Name => "Placed castle structure";
    public bool IsStub => false;
    public string StubReason => "";
    public bool IsLive(GameSnapshot snap) => snap.PlaceLive;

    public bool Satisfied(GameSnapshot snap, RouteCheck check) => Count(snap, check) >= check.Amount;

    public string Status(GameSnapshot snap, RouteCheck check)
    {
        if (!snap.PlaceLive)
        {
            return check.Label + " unread (" + snap.PlaceNote + ")";
        }

        return check.Label + " " + Count(snap, check) + " / " + check.Amount;
    }

    private static int Count(GameSnapshot snap, RouteCheck check)
    {
        var sum = 0;
        foreach (var id in check.Ids)
        {
            snap.Placed.TryGetValue(id, out var have);
            sum += have;
        }

        return sum;
    }
}

public sealed class QuestVerifier : IStepVerifier, IKindVerifier
{
    public CheckKind Kind => CheckKind.Quest;
    public string Name => "Claimed quest";
    public bool IsStub => false;
    public string StubReason => "";
    public bool IsLive(GameSnapshot snap) => snap.QuestLive;

    public bool Satisfied(GameSnapshot snap, RouteCheck check)
    {
        foreach (var id in check.Ids)
        {
            if (!snap.ClaimedQuests.Contains(id))
            {
                return false;
            }
        }

        return check.Ids.Length > 0;
    }

    public string Status(GameSnapshot snap, RouteCheck check)
    {
        if (!snap.QuestLive)
        {
            return check.Label + " unread (" + snap.QuestNote + ")";
        }

        return Satisfied(snap, check) ? check.Label + " claimed" : check.Label + " not claimed";
    }
}

/// <summary>
/// V Rising 1.1.9 client libraries do not include VBloodConsumed / VBloodSystem.EventList.
/// Re-kills cannot be told apart from the permanent unlock, so this stays manual.
/// </summary>
public sealed class KillEventVerifier : IStepVerifier
{
    public string Name => "V Blood kill event";
    public bool IsStub => true;
    public string StubReason =>
        "VBloodConsumed is not in VRising.Unhollowed.Client 1.1.9.9219901. First unlocks use ProgressionUtility.HasUnlockedProgression; a second kill of the same boss stays on confirm.";

    public bool IsLive(GameSnapshot snap) => false;
    public bool Satisfied(GameSnapshot snap, RouteCheck check) => false;
    public string Status(GameSnapshot snap, RouteCheck check) => StubReason;
}

/// <summary>
/// SpellSchoolProgressionUtility_Shared.IsAbilityUnlocked exists, but the prefab hashes for
/// Frost Counter, Frost Barrier, Ward of the Damned, and Crimson Beam were not in the confirmed tables.
/// Those notes stay on screen and are not gates.
/// </summary>
public sealed class SpellUnlockVerifier : IStepVerifier
{
    public string Name => "Named spell unlock";
    public bool IsStub => true;
    public string StubReason =>
        "IsAbilityUnlocked is in the 1.1.9 client libraries, but Frost Counter, Frost Barrier, Ward of the Damned, and Crimson Beam prefab hashes were not confirmed. Spell notes are hints, not gates.";

    public bool IsLive(GameSnapshot snap) => false;
    public bool Satisfied(GameSnapshot snap, RouteCheck check) => false;
    public string Status(GameSnapshot snap, RouteCheck check) => StubReason;
}
