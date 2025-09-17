using UnityEngine;
using VContainer;
using VContainer.Unity;
using MessagePipe;
using CityBuilder.Domain.Models;
using CityBuilder.Repositories;
using CityBuilder.Presentation.Views;
using CityBuilder.Presentation.Presenters;
using CityBuilder.Domain.MessageDTO;
using CityBuilder.Infrastructure.MessagesDTO;
using CityBuilder.Application.UseCases;
using CityBuilder.Presentation.GamePlay.Views;
using CityBuilder.Infrastructure;
using CityBuilder.Presentation.GamePlay.Presenters;
using CityBuilder.Presentation.Gameplay;
using CityBuilder.Presentation.UI.View;
using CityBuilder.Presentation.UI.Presenters;
using CityBuilder.Application.Services;
using CityBuilder.ContractInterfaces;

public class GameplayInstaller : LifetimeScope
{
    [SerializeField] private BuildingConfigurationProvider _buildingConfigProvider;
    protected override void Configure(IContainerBuilder builder)
    {

        this.RegisterDomainLayer(builder);
        this.RegisterRepositoriesLayer(builder);
        this.RegisterInfrastructureLayer(builder);
        this.RegisterApplicationLayer(builder);
        this.RegisterPresentationLayer(builder);

    }

    private void RegisterDomainLayer(IContainerBuilder builder)
    {
        builder.Register<PlayerResourcesModel>(Lifetime.Singleton);
        builder.Register<GridModel>(Lifetime.Singleton);
        builder.Register<PlacementStateModel>(Lifetime.Singleton);
        builder.Register<SelectionModel>(Lifetime.Singleton);
    }

    private void RegisterRepositoriesLayer(IContainerBuilder builder)
    {

        this._buildingConfigProvider.Initialize();
        builder.RegisterInstance(this._buildingConfigProvider);
        
        builder.Register<ISaveLoadService, FileSaveLoadService>(Lifetime.Singleton);


    }

    private void RegisterInfrastructureLayer(IContainerBuilder builder)
    {

        builder.RegisterComponentInHierarchy<InputAdapter>();

        // Регистрируем MessagePipe для обмена сообщениями между слоями
        MessagePipeOptions messagePipeOptions = builder.RegisterMessagePipe();

        // Регистрируем конкретные каналы сообщений.
        // Это нужно для всех DTO, которые вы будете отправлять.
        builder.RegisterMessageBroker<SelectBuildingToPlaceRequestDTO>(messagePipeOptions);
        builder.RegisterMessageBroker<ResourcesUpdatedEventDTO>(messagePipeOptions);
        builder.RegisterMessageBroker<PointerGridPositionChangedDTO>(messagePipeOptions);
        builder.RegisterMessageBroker<PlacementRequestedDTO>(messagePipeOptions);
        builder.RegisterMessageBroker<PlacementCanceledDTO>(messagePipeOptions);
        builder.RegisterMessageBroker<BuildingPlacedEventDTO>(messagePipeOptions);
        builder.RegisterMessageBroker<BuildingPlacementFailedDTO>(messagePipeOptions);
        builder.RegisterMessageBroker<UpgradeSelectedBuildingRequestDTO>(messagePipeOptions);
        builder.RegisterMessageBroker<MoveSelectedBuildingRequestDTO>(messagePipeOptions);
        
        builder.RegisterMessageBroker<CameraControlDTO>(messagePipeOptions);
        builder.RegisterMessageBroker<DeleteSelectedBuildingRequestDTO>(messagePipeOptions);
        builder.RegisterMessageBroker<RotateBuildingRequestDTO>(messagePipeOptions);

        
        builder.RegisterMessageBroker<SaveGameRequestDTO>(messagePipeOptions);
        builder.RegisterMessageBroker<LoadGameRequestDTO>(messagePipeOptions);
        builder.RegisterMessageBroker<GameSavedEventDTO>(messagePipeOptions);
        builder.RegisterMessageBroker<GameLoadedEventDTO>(messagePipeOptions);


    }

    private void RegisterApplicationLayer(IContainerBuilder builder)
    {

        builder.Register<PlaceBuildingUseCase>(Lifetime.Singleton).AsImplementedInterfaces();
        builder.Register<EnterPlacementModeUseCase>(Lifetime.Singleton).AsImplementedInterfaces();

        
        builder.Register<SelectBuildingUseCase>(Lifetime.Singleton).AsImplementedInterfaces();

        // builder.Register<UpgradeBuildingUseCase>(Lifetime.Singleton);
        builder.Register<EconomyService>(Lifetime.Singleton).AsImplementedInterfaces().AsSelf();

        builder.Register<SaveGameUseCase>(Lifetime.Singleton).AsImplementedInterfaces();
        builder.Register<LoadGameUseCase>(Lifetime.Singleton).AsImplementedInterfaces();
        builder.Register<AutoSaveService>(Lifetime.Singleton).AsImplementedInterfaces();
    }

    private void RegisterPresentationLayer(IContainerBuilder builder)
    {

        builder.RegisterComponentInHierarchy<GridView>();
        builder.Register<GridPresenter>(Lifetime.Singleton).AsImplementedInterfaces().AsSelf();

        builder.RegisterComponentInHierarchy<HudView>();
        builder.Register<HudPresenter>(Lifetime.Singleton).AsImplementedInterfaces().AsSelf();
        
        builder.RegisterComponentInHierarchy<BuildingContextMenuView>();
        builder.Register<BuildingContextMenuPresenter>(Lifetime.Singleton).AsImplementedInterfaces().AsSelf();

        
        builder.RegisterComponentInHierarchy<BuildingTooltipView>();
        builder.Register<BuildingTooltipPresenter>(Lifetime.Singleton).AsImplementedInterfaces().AsSelf();
        

        // Регистрируем Presenter. VContainer сам "подтянет" HudView, 
        // MessagePipe и другие зависимости в его конструктор.
        // AsImplementedInterfaces() заставит VContainer автоматически вызвать
        // методы Initialize() и Dispose() у HudPresenter.

        builder.RegisterComponentInHierarchy<CameraOperator>().AsImplementedInterfaces();
    }
}