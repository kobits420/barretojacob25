using SpeedrunOverlay.Game;
using SpeedrunOverlay.Verify;

namespace SpeedrunOverlay.Route;

public sealed class RouteRunner
{
    private readonly RouteStep[] _steps;
    private bool _baselined;
    private bool _wasSatisfied;

    public RouteRunner(RouteStep[] steps, int startIndex)
    {
        _steps = steps;
        Index = Math.Clamp(startIndex, 0, steps.Length - 1);
    }

    public int Index { get; private set; }
    public RouteStep Current => _steps[Index];
    public int Count => _steps.Length;

    public event Action? Changed;

    public void Confirm()
    {
        if (Index >= _steps.Length - 1)
        {
            return;
        }

        Index++;
        ResetBaseline();
        Changed?.Invoke();
    }

    public void Back()
    {
        if (Index <= 0)
        {
            return;
        }

        Index--;
        ResetBaseline();
        Changed?.Invoke();
    }

    public void Tick(GameSnapshot snap)
    {
        var step = Current;
        if (!step.CanAutoComplete || !snap.HasCharacter)
        {
            return;
        }

        if (!ProbesLive(snap, step))
        {
            return;
        }

        var satisfied = AllSatisfied(snap, step);
        if (!_baselined)
        {
            _wasSatisfied = satisfied;
            _baselined = true;
            return;
        }

        if (!satisfied)
        {
            _wasSatisfied = false;
            return;
        }

        if (_wasSatisfied)
        {
            return;
        }

        if (Index >= _steps.Length - 1)
        {
            _wasSatisfied = true;
            return;
        }

        Index++;
        ResetBaseline();
        Changed?.Invoke();
    }

    public string GateStatus(GameSnapshot snap)
    {
        var step = Current;
        if (step.Conditional)
        {
            return "Conditional step. It stays until you confirm. It will not be skipped.";
        }

        if (!step.CanAutoComplete)
        {
            return "Manual. Confirm when you have done it.";
        }

        if (!_baselined)
        {
            return ProbesLive(snap, step)
                ? "Reading the step. It advances when the check becomes true."
                : "Waiting for a live game read. Confirm if you are already done.";
        }

        if (_wasSatisfied && AllSatisfied(snap, step))
        {
            return "Already true when this step became current. Confirm to accept it.";
        }

        var parts = new List<string>();
        foreach (var check in step.Checks)
        {
            parts.Add(VerifierSet.For(check.Kind).Status(snap, check));
        }

        return string.Join("  ·  ", parts);
    }

    private void ResetBaseline()
    {
        _baselined = false;
        _wasSatisfied = false;
    }

    private static bool ProbesLive(GameSnapshot snap, RouteStep step)
    {
        foreach (var check in step.Checks)
        {
            var verifier = VerifierSet.For(check.Kind);
            if (verifier.IsStub || !verifier.IsLive(snap))
            {
                return false;
            }
        }

        return true;
    }

    private static bool AllSatisfied(GameSnapshot snap, RouteStep step)
    {
        foreach (var check in step.Checks)
        {
            var verifier = VerifierSet.For(check.Kind);
            if (!verifier.Satisfied(snap, check))
            {
                return false;
            }
        }

        return true;
    }
}
