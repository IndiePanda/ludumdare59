using System;
using System.Collections.Generic;
using Zenject;

public class EnergyRestorer : IInitializable, IDisposable
{
    private readonly MusicBoxZone _musicBoxZone;
    private readonly EnergyZone _energyZone;
    private readonly EnergySystem _energySystem;

    public EnergyRestorer(
        MusicBoxZone musicBoxZone,
        EnergyZone energyZone, 
        EnergySystem energySystem
        )
    {
        _musicBoxZone = musicBoxZone;
        _energyZone = energyZone;
        _energySystem = energySystem;
    }

    public void Initialize()
    {
        _musicBoxZone.EnergyRestoreRequested += OnEnergyRestoreRequested;
        _energyZone.EnergyRestoreRequested += OnEnergyRestoreRequested;
    }

    private void OnEnergyRestoreRequested()
    {
        _energySystem.AddEnergy(1);
    }

    public void Dispose()
    {
        _musicBoxZone.EnergyRestoreRequested -= OnEnergyRestoreRequested;
        _energyZone.EnergyRestoreRequested -= OnEnergyRestoreRequested;
    }
}
