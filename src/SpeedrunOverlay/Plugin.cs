using BepInEx;
using BepInEx.Logging;
using BepInEx.Unity.IL2CPP;
using Il2CppInterop.Runtime.Injection;
using SpeedrunOverlay.Hud;
using SpeedrunOverlay.Route;
using SpeedrunOverlay.Verify;

namespace SpeedrunOverlay;

[BepInPlugin(PluginGuid, PluginName, PluginVersion)]
public sealed class Plugin : BasePlugin
{
    public const string PluginGuid = "kobits420.SpeedrunOverlay";
    public const string PluginName = "V Rising Speedrun Overlay";
    public const string PluginVersion = "1.0.0";

    public static new ManualLogSource? Log { get; private set; }

    private OverlayBehaviour? _behaviour;

    public override void Load()
    {
        Log = base.Log;
        try
        {
            var config = new OverlayConfig(Config);
            var pins = new PinStore(Path.Combine(Paths.ConfigPath, PluginGuid + ".pins.cfg"));
            var runner = new RouteRunner(RouteCatalog.Steps, config.CurrentStep.Value - 1);
            runner.Changed += () => config.CurrentStep.Value = runner.Index + 1;

            ClassInjector.RegisterTypeInIl2Cpp<OverlayBehaviour>();
            _behaviour = AddComponent<OverlayBehaviour>();
            _behaviour.Bind(config, runner, pins);

            Log.LogInfo($"{PluginName} {PluginVersion} loaded. Route steps: {RouteCatalog.Steps.Length}.");
            foreach (var verifier in VerifierSet.All)
            {
                if (verifier.IsStub)
                {
                    Log.LogInfo("Stubbed verifier: " + verifier.Name + ". " + verifier.StubReason);
                }
                else
                {
                    Log.LogInfo("Live verifier: " + verifier.Name + ".");
                }
            }
        }
        catch (Exception ex)
        {
            Log.LogError("Speedrun overlay failed to start: " + ex);
        }
    }

    public override bool Unload()
    {
        if (_behaviour != null)
        {
            UnityEngine.Object.Destroy(_behaviour);
            _behaviour = null;
        }

        return true;
    }
}
