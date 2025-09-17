using MessagePipe;
using System.Linq;
using VContainer.Unity;
using System;
using Cysharp.Threading.Tasks;
using CityBuilder.Domain.Models;
using CityBuilder.Domain.MessageDTO;
using CityBuilder.Application.Services;
using CityBuilder.ContractInterfaces;

// Определим DTO для запросов
public struct SaveGameRequestDTO { }
public struct GameSavedEventDTO { }

public class SaveGameUseCase : IInitializable, IDisposable
{
    private readonly GridModel _gridModel;
    private readonly PlayerResourcesModel _resourcesModel;
    private readonly ISaveLoadService _saveLoadService;
    private readonly ISubscriber<SaveGameRequestDTO> _saveRequestSubscriber;
    private readonly IPublisher<GameSavedEventDTO> _gameSavedPublisher;
    private IDisposable _disposable;

    public SaveGameUseCase(GridModel gridModel, PlayerResourcesModel resourcesModel, ISaveLoadService saveLoadService, ISubscriber<SaveGameRequestDTO> saveRequestSubscriber, IPublisher<GameSavedEventDTO> gameSavedPublisher)
    {

        this._gridModel = gridModel;
        this._resourcesModel = resourcesModel;
        this._saveLoadService = saveLoadService;
        this._saveRequestSubscriber = saveRequestSubscriber;
        this._gameSavedPublisher = gameSavedPublisher;
    }

    public void Initialize()
    {
        this._disposable = this._saveRequestSubscriber.Subscribe(_ => this.SaveGame().Forget());
    }

    private async UniTaskVoid SaveGame()
    {
        CityBuilder.Domain.Models.SaveData saveData = new SaveData();

        saveData.Buildings = this._gridModel.GetAllBuildings().Select(b => new BuildingSaveData
        {
            InstanceId = b.InstanceId,
            TypeId = (int)b.Type,
            Level = b.Level,
            Position = b.Position
        }).ToList();

        foreach (ResourceType type in Enum.GetValues(typeof(ResourceType)))
        {
            int amount = this._resourcesModel.GetAmount(type);
            if (amount > 0)
            {
                saveData.Resources.Add(new ResourceSaveData { ResourceType = type, Amount = amount });
            }
        }

        await this._saveLoadService.SaveAsync(saveData);

        this._gameSavedPublisher.Publish(new GameSavedEventDTO());
    }

    public void Dispose() => this._disposable?.Dispose();
}

public struct LoadGameRequestDTO { }
public struct GameLoadedEventDTO { }

public class LoadGameUseCase : IInitializable, IDisposable
{

    private readonly GridModel _gridModel;
    private readonly PlayerResourcesModel _resourcesModel;
    private readonly ISaveLoadService _saveLoadService;
    private readonly ISubscriber<LoadGameRequestDTO> _loadRequestSubscriber;
    private readonly IPublisher<GameLoadedEventDTO> _gameLoadedPublisher;
    private IDisposable _disposable;

    public LoadGameUseCase(GridModel gridModel, PlayerResourcesModel resourcesModel, ISaveLoadService saveLoadService, ISubscriber<LoadGameRequestDTO> loadRequestSubscriber, IPublisher<GameLoadedEventDTO> gameLoadedPublisher)
    {
        this._saveLoadService = saveLoadService;
        this._loadRequestSubscriber = loadRequestSubscriber;
        this._gameLoadedPublisher = gameLoadedPublisher;
        this._gridModel = gridModel;
        this._resourcesModel = resourcesModel;
    }



    public void Initialize()
    {
        this._disposable = this._loadRequestSubscriber.Subscribe(_ => this.LoadGame().Forget());
    }

    private async UniTaskVoid LoadGame()
    {
        SaveData saveData = await this._saveLoadService.LoadAsync();
        if (saveData == null) return;

        // 1. Очищаем текущее состояние (в GridModel и ResourcesModel нужны методы Clear)
        this._gridModel.Clear();
        this._resourcesModel.Clear();

        // 2. Восстанавливаем ресурсы
        foreach (ResourceSaveData resourceData in saveData.Resources)
        {
            this._resourcesModel.SetAmount(resourceData.ResourceType, resourceData.Amount);
        }

        // 3. Восстанавливаем здания
        foreach (BuildingSaveData buildingData in saveData.Buildings)
        {
            BuildingInstanceModel newBuilding = new BuildingInstanceModel(
                buildingData.InstanceId,
                (BuildingType)buildingData.TypeId,
                buildingData.Position,
                buildingData.Level);
            this._gridModel.AddBuilding(newBuilding);
        }

        // 4. Уведомляем всю систему, что игра перезагружена
        this._gameLoadedPublisher.Publish(new GameLoadedEventDTO());
    }

    public void Dispose() => this._disposable?.Dispose();
}