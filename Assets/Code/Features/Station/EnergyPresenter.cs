public class EnergyPresenter
{
    private EnergySystem _energySystem;
    private EnergyView _energyView;

    public EnergyPresenter(
        EnergySystem energySystem,
        EnergyView energyView
        )
    {
        _energySystem = energySystem;
        _energyView = energyView;
        _energySystem.ChangeEnergy += OnEnergyChanged;
    }

    private void OnEnergyChanged(int value)
    {
        _energyView.UpdateEnergy(value);
    }
}

