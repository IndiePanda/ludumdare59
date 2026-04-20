using System;
using Zenject;

public class EnergyRestorer : IInitializable, IDisposable
{
    private readonly EnergyZone _energyZone;
    private readonly EnergySystem _energySystem;

    public EnergyRestorer(
        EnergyZone energyZone, 
        EnergySystem energySystem
        )
    {

        _energyZone = energyZone;
        _energySystem = energySystem;
    }

    public void Initialize()
    {
        _energyZone.EnergyRestoreRequested += OnEnergyRestoreRequested;
    }

    private void OnEnergyRestoreRequested()
    {
        _energySystem.AddEnergy(1);
    }

    public void Dispose()
    {
        _energyZone.EnergyRestoreRequested -= OnEnergyRestoreRequested;
    }
}
