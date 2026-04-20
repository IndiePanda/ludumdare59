using UnityEngine;
using Zenject;

public class Installer : MonoInstaller
{
    [SerializeField] private UIConfig _uiConfig;
    [SerializeField] private Transform _canvas;
    [Header("Prefabs")]
    [SerializeField] private GameObject _roomCheckInButtonPrefab;
    [Header("SceneObjects")]
    [SerializeField] private GameObject _dialogVariant;

    public override void InstallBindings()
    {
        Container.BindInterfacesAndSelfTo<GameBootstrapper>().FromComponentInHierarchy().AsSingle().NonLazy();

        //Base
        Container.Bind<UIConfig>().FromInstance(_uiConfig).AsSingle();
        Container.Bind<Transform>().WithId("CANVAS").FromInstance(_canvas).AsSingle();
        Container.Bind<TriggerPopupHandler>().AsSingle();

        //Station
        Container.Bind<StationManager>().FromComponentInHierarchy().AsSingle().NonLazy();
        Container.Bind<MusicBoxZone>().FromComponentsInHierarchy().AsCached();
        Container.Bind<EnergyZone>().FromComponentsInHierarchy().AsCached();
        Container.BindInterfacesAndSelfTo<EnergyRestorer>().AsSingle().NonLazy();

        //Sugnals
        Container.BindInterfacesAndSelfTo<DaySystem>().AsSingle().NonLazy();
        Container.BindInterfacesAndSelfTo<SignalSystem>().AsSingle().NonLazy();
        Container.BindInterfacesTo<DecodingSystem>().AsSingle().NonLazy();

        //Energy
        Container.Bind<EnergySystem>().AsSingle().NonLazy();
        Container.Bind<EnergyView>().FromComponentInHierarchy().AsSingle().NonLazy();
        Container.BindInterfacesAndSelfTo<EnergyPresenter>().AsSingle().NonLazy();

        //Timer
        Container.Bind<TimerView>().FromComponentInHierarchy().AsSingle().NonLazy();
        Container.BindInterfacesAndSelfTo<TimerPresenter>().AsSingle().NonLazy();

        //Tutorial
        Container.Bind<TutorialSystem>().AsSingle().NonLazy();
        Container.Bind<TutorialView>().FromComponentInHierarchy().AsSingle().NonLazy();
 
    }
}
