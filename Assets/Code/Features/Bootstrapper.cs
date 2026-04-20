using UnityEngine;
using Zenject;

public class GameBootstrapper : MonoBehaviour, IInitializable
{
    [Inject] private EnergySystem _energySystem;

    public void Initialize()
    {
        _energySystem.Initialize();

    }
}
