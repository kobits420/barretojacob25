using Il2CppInterop.Runtime;
using ProjectM;
using ProjectM.CastleBuilding;
using ProjectM.Terrain;
using Stunlock.Core;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;

namespace SpeedrunOverlay.Game;

/// <summary>
/// Reads the local client world. Each probe is isolated: a failure marks that probe
/// not live and leaves the step on manual confirm. The plugin still loads.
/// </summary>
public sealed class GameReader
{
    private static readonly HashSet<int> TrackedItems = BuildTrackedItems();
    private static readonly HashSet<int> TrackedPlaced = BuildTrackedPlaced();
    private static readonly HashSet<int> TrackedEquip = BuildTrackedEquip();

    private readonly Dictionary<string, string> _logged = new();
    private EntityQuery _heartQuery;
    private EntityQuery _linkQuery;
    private EntityQuery _regionQuery;
    private EntityQuery _markQuery;
    private EntityQuery _bloodQuery;
    private World? _queriesWorld;
    private bool _queriesReady;

    public void Refresh(GameSnapshot snap, int[]? highlightIds, int bloodTypeGuid)
    {
        snap.Clear();
        var world = FindClientWorld();
        if (world == null || !world.IsCreated)
        {
            return;
        }

        snap.InWorld = true;
        var em = world.EntityManager;
        EnsureQueries(world, em);

        Entity character = Entity.Null;
        Entity user = Entity.Null;
        try
        {
            ConsoleShared.TryGetLocalCharacterInCurrentWorld(out character, world);
            ConsoleShared.TryGetLocalUserInCurrentWorld(out user, world);
        }
        catch (Exception ex)
        {
            LogOnce("local", "Local character lookup failed: " + ex.Message);
            return;
        }

        if (character == Entity.Null)
        {
            return;
        }

        snap.HasCharacter = true;
        ReadPosition(em, character, snap);
        ReadGearAndEquipment(em, character, snap);
        ReadVBlood(em, character, snap);
        ReadRegion(em, character, user, snap);
        ReadHeartAndPlaced(em, character, user, snap);
        ReadInventory(em, character, user, snap);
        ReadQuests(em, character, user, snap);
        if (highlightIds is { Length: > 0 })
        {
            ReadMarks(em, character, snap, highlightIds);
        }

        if (bloodTypeGuid != 0)
        {
            ReadBloodTarget(em, character, snap, bloodTypeGuid);
        }
    }

    public static World? FindClientWorld()
    {
        try
        {
            var world = World.FindWorld(WorldType.Client, false);
            if (world != null && world.IsCreated)
            {
                return world;
            }
        }
        catch (Exception)
        {
            // FindWorld is the confirmed lookup. Fall through to the world list.
        }

        try
        {
            foreach (var world in World.All)
            {
                if (world != null && world.IsCreated && world.Name != null &&
                    world.Name.StartsWith("Client", StringComparison.Ordinal))
                {
                    return world;
                }
            }
        }
        catch (Exception)
        {
            return null;
        }

        return null;
    }

    private void EnsureQueries(World world, EntityManager em)
    {
        if (_queriesReady && ReferenceEquals(_queriesWorld, world))
        {
            return;
        }

        DisposeQueries();
        _heartQuery = Create(em, Il2CppType.Of<CastleHeart>());
        _linkQuery = Create(em, Il2CppType.Of<CastleHeartConnection>(), Il2CppType.Of<PrefabGUID>());
        _regionQuery = Create(em, Il2CppType.Of<CurrentWorldRegion>());
        _markQuery = Create(em, Il2CppType.Of<PrefabGUID>(), Il2CppType.Of<LocalTransform>());
        _bloodQuery = Create(em, Il2CppType.Of<BloodConsumeSource>(), Il2CppType.Of<LocalTransform>());
        _queriesWorld = world;
        _queriesReady = true;
    }

    private static EntityQuery Create(EntityManager em, Il2CppSystem.Type type)
    {
        var builder = new EntityQueryBuilder(Allocator.Temp)
            .AddAll(new ComponentType(type, ComponentType.AccessMode.ReadOnly));
        return em.CreateEntityQuery(ref builder);
    }

    private static EntityQuery Create(EntityManager em, Il2CppSystem.Type a, Il2CppSystem.Type b)
    {
        var builder = new EntityQueryBuilder(Allocator.Temp)
            .AddAll(new ComponentType(a, ComponentType.AccessMode.ReadOnly))
            .AddAll(new ComponentType(b, ComponentType.AccessMode.ReadOnly));
        return em.CreateEntityQuery(ref builder);
    }

    private void DisposeQueries()
    {
        if (!_queriesReady)
        {
            return;
        }

        TryDispose(_heartQuery);
        TryDispose(_linkQuery);
        TryDispose(_regionQuery);
        TryDispose(_markQuery);
        TryDispose(_bloodQuery);
        _queriesReady = false;
        _queriesWorld = null;
    }

    private static void TryDispose(EntityQuery query)
    {
        try
        {
            query.Dispose();
        }
        catch (Exception)
        {
            // Query may already belong to a destroyed world.
        }
    }

    private void ReadPosition(EntityManager em, Entity character, GameSnapshot snap)
    {
        try
        {
            if (!em.HasComponent<LocalTransform>(character))
            {
                return;
            }

            float3 pos = em.GetComponentData<LocalTransform>(character).Position;
            snap.PositionLive = true;
            snap.X = pos.x;
            snap.Y = pos.y;
            snap.Z = pos.z;
        }
        catch (Exception ex)
        {
            LogOnce("pos", ex.Message);
        }
    }

    private void ReadGearAndEquipment(EntityManager em, Entity character, GameSnapshot snap)
    {
        try
        {
            if (!em.HasComponent<Equipment>(character))
            {
                snap.GearNote = "Equipment is not on the local character.";
                return;
            }

            var equipment = em.GetComponentData<Equipment>(character);
            snap.GearScore = (int)MathF.Round(equipment.GetFullLevel());
            snap.GearLive = true;
            foreach (var id in TrackedEquip)
            {
                if (equipment.IsEquipped(new PrefabGUID(id), out _))
                {
                    snap.Equipped.Add(id);
                }
            }

            snap.EquipLive = true;
        }
        catch (Exception ex)
        {
            snap.GearNote = ex.Message;
            LogOnce("gear", ex.Message);
        }
    }

    private void ReadVBlood(EntityManager em, Entity character, GameSnapshot snap)
    {
        try
        {
            foreach (var id in PrefabCatalog.RouteVBloods)
            {
                if (ProgressionUtility.HasUnlockedProgression(em, character, new PrefabGUID(id)))
                {
                    snap.UnlockedVBloods.Add(id);
                }
            }

            snap.VBloodLive = true;
        }
        catch (Exception ex)
        {
            snap.VBloodNote = ex.Message;
            LogOnce("vblood", ex.Message);
        }
    }

    private void ReadRegion(EntityManager em, Entity character, Entity user, GameSnapshot snap)
    {
        try
        {
            if (TryRegion(em, character, out var region) || TryRegion(em, user, out region))
            {
                ApplyRegion(snap, region);
                return;
            }

            var regions = _regionQuery.ToComponentDataArray<CurrentWorldRegion>(Allocator.Temp);
            try
            {
                if (regions.Length == 1)
                {
                    ApplyRegion(snap, regions[0].CurrentRegion);
                    return;
                }
            }
            finally
            {
                regions.Dispose();
            }

            snap.RegionNote = "CurrentWorldRegion is not on the local character or user.";
        }
        catch (Exception ex)
        {
            snap.RegionNote = ex.Message;
            LogOnce("region", ex.Message);
        }
    }

    private static bool TryRegion(EntityManager em, Entity entity, out WorldRegionType region)
    {
        region = WorldRegionType.None;
        if (entity == Entity.Null || !em.HasComponent<CurrentWorldRegion>(entity))
        {
            return false;
        }

        region = em.GetComponentData<CurrentWorldRegion>(entity).CurrentRegion;
        return true;
    }

    private static void ApplyRegion(GameSnapshot snap, WorldRegionType region)
    {
        snap.RegionLive = true;
        snap.Region = Convert.ToInt32(region);
        snap.RegionName = region.ToString();
    }

    private void ReadHeartAndPlaced(EntityManager em, Entity character, Entity user, GameSnapshot snap)
    {
        try
        {
            var hearts = _heartQuery.ToEntityArray(Allocator.Temp);
            var owned = new HashSet<Entity>();
            var best = 0;
            var any = false;
            for (var i = 0; i < hearts.Length; i++)
            {
                var heart = hearts[i];
                if (!OwnsHeart(em, character, user, heart))
                {
                    continue;
                }

                any = true;
                owned.Add(heart);
                var level = Convert.ToInt32(em.GetComponentData<CastleHeart>(heart).Level);
                if (level > best)
                {
                    best = level;
                }
            }

            if (!any)
            {
                snap.HeartNote = "No castle heart owned by the local user was found.";
                snap.PlaceNote = snap.HeartNote;
                hearts.Dispose();
                return;
            }

            snap.HeartLive = true;
            snap.HeartLevel = best;

            var linked = _linkQuery.ToEntityArray(Allocator.Temp);
            try
            {
            for (var i = 0; i < linked.Length; i++)
            {
                var entity = linked[i];
                var heart = em.GetComponentData<CastleHeartConnection>(entity).CastleHeartEntity._Entity;
                if (!owned.Contains(heart))
                {
                    continue;
                }

                var id = em.GetComponentData<PrefabGUID>(entity).GuidHash;
                if (!TrackedPlaced.Contains(id))
                {
                    continue;
                }

                snap.Placed.TryGetValue(id, out var count);
                snap.Placed[id] = count + 1;
            }

            snap.PlaceLive = true;
            }
            finally
            {
                linked.Dispose();
                hearts.Dispose();
            }
        }
        catch (Exception ex)
        {
            snap.HeartNote = ex.Message;
            snap.PlaceNote = ex.Message;
            LogOnce("heart", ex.Message);
        }
    }

    private static bool OwnsHeart(EntityManager em, Entity character, Entity user, Entity heart)
    {
        if (user != Entity.Null && em.HasComponent<CastleHeart>(heart))
        {
            var owner = em.GetComponentData<CastleHeart>(heart).LastUserOwner._Entity;
            if (owner == user)
            {
                return true;
            }
        }

        if (character == Entity.Null || !em.HasComponent<TeamReference>(character) ||
            !em.HasComponent<CastleHeartCurrentTeam>(heart))
        {
            return false;
        }

        var playerTeam = em.GetComponentData<TeamReference>(character).Value;
        var heartTeam = em.GetComponentData<CastleHeartCurrentTeam>(heart).Team;
        return playerTeam.Equals(heartTeam);
    }

    private void ReadInventory(EntityManager em, Entity character, Entity user, GameSnapshot snap)
    {
        try
        {
            var found = false;
            found |= CollectItems(em, character, snap);
            found |= CollectItems(em, user, snap);
            if (!found)
            {
                snap.InventoryNote = "InventoryBuffer is not on the local character or user.";
                return;
            }

            snap.InventoryLive = true;
        }
        catch (Exception ex)
        {
            snap.InventoryNote = ex.Message;
            LogOnce("inv", ex.Message);
        }
    }

    private static bool CollectItems(EntityManager em, Entity entity, GameSnapshot snap)
    {
        if (entity == Entity.Null || !em.HasBuffer<InventoryBuffer>(entity))
        {
            return false;
        }

        var buffer = em.GetBuffer<InventoryBuffer>(entity);
        for (var i = 0; i < buffer.Length; i++)
        {
            var slot = buffer[i];
            if (slot.Amount <= 0)
            {
                continue;
            }

            var id = slot.ItemType.GuidHash;
            if (!TrackedItems.Contains(id))
            {
                continue;
            }

            snap.Items.TryGetValue(id, out var have);
            snap.Items[id] = have + slot.Amount;
        }

        return true;
    }

    private void ReadQuests(EntityManager em, Entity character, Entity user, GameSnapshot snap)
    {
        try
        {
            var found = false;
            found |= CollectQuests(em, user, snap);
            found |= CollectQuests(em, character, snap);
            if (ProgressionUtility.TryGetProgressionEntity(em, character, out var progression))
            {
                found |= CollectQuests(em, progression, snap);
            }

            if (!found)
            {
                snap.QuestNote = "AchievementClaimedElement was not found for the local player.";
                return;
            }

            snap.QuestLive = true;
        }
        catch (Exception ex)
        {
            snap.QuestNote = ex.Message;
            LogOnce("quest", ex.Message);
        }
    }

    private static bool CollectQuests(EntityManager em, Entity entity, GameSnapshot snap)
    {
        if (entity == Entity.Null || !em.HasBuffer<AchievementClaimedElement>(entity))
        {
            return false;
        }

        var buffer = em.GetBuffer<AchievementClaimedElement>(entity);
        for (var i = 0; i < buffer.Length; i++)
        {
            snap.ClaimedQuests.Add(buffer[i].AchievementPrefabGuid.GuidHash);
        }

        return true;
    }

    private void ReadBloodTarget(EntityManager em, Entity character, GameSnapshot snap, int bloodTypeGuid)
    {
        snap.BloodTypeGuid = bloodTypeGuid;
        try
        {
            var sources = _bloodQuery.ToEntityArray(Allocator.Temp);
            try
            {
                var bestDistance = float.MaxValue;
                var found = false;
                float bestX = 0f, bestY = 0f, bestZ = 0f, bestQuality = 0f;
                for (var i = 0; i < sources.Length; i++)
                {
                    var entity = sources[i];
                    if (entity == character)
                    {
                        continue;
                    }

                    var source = em.GetComponentData<BloodConsumeSource>(entity);
                    if (!source.CanBeConsumed || source.ForceBadBloodQuality || source.BloodQuality < BloodTypes.PerfectQuality)
                    {
                        continue;
                    }

                    if (source.UnitBloodType.Value.GuidHash != bloodTypeGuid)
                    {
                        continue;
                    }

                    if (em.HasComponent<Health>(entity) && em.GetComponentData<Health>(entity).IsDead)
                    {
                        continue;
                    }

                    var pos = em.GetComponentData<LocalTransform>(entity).Position;
                    var dx = pos.x - snap.X;
                    var dy = pos.y - snap.Y;
                    var dz = pos.z - snap.Z;
                    var distance = MathF.Sqrt(dx * dx + dy * dy + dz * dz);
                    if (distance >= bestDistance)
                    {
                        continue;
                    }

                    bestDistance = distance;
                    bestX = pos.x;
                    bestY = pos.y;
                    bestZ = pos.z;
                    bestQuality = source.BloodQuality;
                    found = true;
                }

                snap.BloodSearchLive = true;
                snap.BloodTargetFound = found && snap.PositionLive;
                if (snap.BloodTargetFound)
                {
                    snap.BloodX = bestX;
                    snap.BloodY = bestY;
                    snap.BloodZ = bestZ;
                    snap.BloodQuality = bestQuality;
                    snap.BloodDistance = bestDistance;
                }
            }
            finally
            {
                sources.Dispose();
            }
        }
        catch (Exception ex)
        {
            snap.BloodNote = ex.Message;
            LogOnce("blood", ex.Message);
        }
    }

    private void ReadMarks(EntityManager em, Entity character, GameSnapshot snap, int[] highlightIds)
    {
        if (!snap.PositionLive)
        {
            return;
        }

        try
        {
            var wanted = new HashSet<int>(highlightIds);
            var entities = _markQuery.ToEntityArray(Allocator.Temp);
            var best = new List<WorldMark>(24);
            try
            {
            for (var i = 0; i < entities.Length; i++)
            {
                var entity = entities[i];
                if (entity == character)
                {
                    continue;
                }

                var id = em.GetComponentData<PrefabGUID>(entity).GuidHash;
                if (!wanted.Contains(id))
                {
                    continue;
                }

                var pos = em.GetComponentData<LocalTransform>(entity).Position;
                var dx = pos.x - snap.X;
                var dy = pos.y - snap.Y;
                var dz = pos.z - snap.Z;
                var distance = MathF.Sqrt(dx * dx + dy * dy + dz * dz);
                if (distance > 280f)
                {
                    continue;
                }

                InsertMark(best, new WorldMark(id, pos.x, pos.y, pos.z, distance));
            }

            snap.Marks.AddRange(best);
            }
            finally
            {
                entities.Dispose();
            }
        }
        catch (Exception ex)
        {
            LogOnce("marks", ex.Message);
        }
    }

    private static void InsertMark(List<WorldMark> best, WorldMark mark)
    {
        if (best.Count == 24 && mark.Distance >= best[^1].Distance)
        {
            return;
        }

        var index = best.FindIndex(existing => existing.Distance > mark.Distance);
        if (index < 0)
        {
            best.Add(mark);
        }
        else
        {
            best.Insert(index, mark);
        }

        if (best.Count > 24)
        {
            best.RemoveAt(best.Count - 1);
        }
    }

    private void LogOnce(string key, string message)
    {
        if (_logged.ContainsKey(key))
        {
            return;
        }

        _logged[key] = message;
        Plugin.Log?.LogWarning($"Speedrun overlay probe '{key}' is not live: {message}");
    }

    private static HashSet<int> BuildTrackedItems()
    {
        var set = new HashSet<int>
        {
            PrefabCatalog.CottonYarn,
            PrefabCatalog.PristineHide,
            PrefabCatalog.ShroudOfTheForest,
            PrefabCatalog.SludgeCanister,
            PrefabCatalog.Emery,
            PrefabCatalog.Battery,
            PrefabCatalog.ChargedBattery,
            PrefabCatalog.OnyxTear,
        };
        foreach (var id in PrefabCatalog.Amulets)
        {
            set.Add(id);
        }

        foreach (var id in PrefabCatalog.DarkSilverWeapons)
        {
            set.Add(id);
        }

        return set;
    }

    private static HashSet<int> BuildTrackedPlaced()
    {
        var set = new HashSet<int>
        {
            PrefabCatalog.ResearchDesk,
            PrefabCatalog.Study,
            PrefabCatalog.Athenaeum,
            PrefabCatalog.Smithy,
            PrefabCatalog.TailoringBench,
            PrefabCatalog.ArtisanTable,
            PrefabCatalog.StoneCoffin,
            PrefabCatalog.Fabricator,
            PrefabCatalog.AdvancedBloodPress,
            PrefabCatalog.PrisonCell,
            PrefabCatalog.VerminNest,
            PrefabCatalog.Anvil,
            PrefabCatalog.AlchemyTable,
            PrefabCatalog.Stables,
            PrefabCatalog.CastleWaygate,
        };
        foreach (var id in PrefabCatalog.Sawmills) set.Add(id);
        foreach (var id in PrefabCatalog.Furnaces) set.Add(id);
        foreach (var id in PrefabCatalog.Looms) set.Add(id);
        return set;
    }

    private static HashSet<int> BuildTrackedEquip()
    {
        var set = new HashSet<int> { PrefabCatalog.ShroudOfTheForest };
        foreach (var id in PrefabCatalog.DarkSilverWeapons)
        {
            set.Add(id);
        }

        return set;
    }
}
