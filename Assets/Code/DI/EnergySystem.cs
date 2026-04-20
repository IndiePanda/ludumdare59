using System;

public class EnergySystem : IDisposable
{
    private int _currentEnergy = 0;
    public int CurrentEnergy => _currentEnergy;
    public event Action<int> ChangeEnergy;

    public void Initialize()
    {
        ChangeEnergy?.Invoke(_currentEnergy);
    }

    public void AddEnergy(int value)
    {
        if (value <= 0)
        {
            return;
        }

        _currentEnergy += value;
        ChangeEnergy?.Invoke(_currentEnergy);
    }

    public bool TrySpendEnergy(int value)
    {
        if (value <= 0 || _currentEnergy < value)
        {
            return false;
        }

        _currentEnergy -= value;
        ChangeEnergy?.Invoke(_currentEnergy);
        return true;
    }

    public void Dispose()
    {

    }
}