using UnityEngine;
using Zenject;

public class Installer : MonoInstaller
{
    [SerializeField] private UIConfig _uiConfig;
    [SerializeField] private MessageSchedule _messageSchedule;
    [SerializeField] private TutorialTextBase _tutorialTextBase;
    [SerializeField] private SFXAudio _sfxAudio;
    [SerializeField] private Transform _canvas;
    //[Header("Prefabs")]
    //[Header("SceneObjects")]


    public override void InstallBindings()
    {
        Container.BindInterfacesAndSelfTo<GameBootstrapper>().FromComponentInHierarchy().AsSingle().NonLazy();

        //Base
        Container.Bind<UIConfig>().FromInstance(_uiConfig).AsSingle();
        Container.Bind<SFXAudio>().FromInstance(_sfxAudio).AsSingle();
        Container.Bind<MessageSchedule>().FromInstance(_messageSchedule).AsSingle();
        Container.Bind<TutorialTextBase>().FromInstance(_tutorialTextBase).AsSingle();
        Container.Bind<Transform>().WithId("CANVAS").FromInstance(_canvas).AsSingle();
        Container.Bind<TriggerPopupHandler>().AsSingle();

        //Station
        Container.Bind<StationManager>().FromComponentInHierarchy().AsSingle().NonLazy();
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
        Container.BindInterfacesAndSelfTo<TutorialSystem>().AsSingle().NonLazy();
        Container.Bind<TutorialView>().FromComponentInHierarchy().AsSingle().NonLazy();
        Container.Bind<CompletePanelView>().FromComponentInHierarchy().AsSingle().NonLazy();
 
    }
}
