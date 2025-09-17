using System;
using System.Threading;
using VContainer.Unity;
using MessagePipe;
using Cysharp.Threading.Tasks;
using CityBuilder.Domain.Models;
using CityBuilder.Domain.MessageDTO;
using CityBuilder.Repositories;
using CityBuilder.Domain.Models;
using CityBuilder.Repositories;
using System.Collections.Generic;
using CityBuilder.Infrastructure.MessagesDTO;

namespace CityBuilder.Application.Services
{
    /// <summary>
    /// Сервис, отвечающий за фоновое начисление ресурсов от зданий.
    /// </summary>
    public class EconomyService : IInitializable, IDisposable
    {
        private static readonly double _tickIntervalSeconds = 5.0f;

        private readonly GridModel _gridModel;
        private readonly PlayerResourcesModel _resourcesModel;
        private readonly BuildingConfigurationProvider _configProvider;
        private readonly IPublisher<ResourcesUpdatedEventDTO> _resourcesUpdatedPublisher;

        private readonly CancellationTokenSource _cancellation = new();

        public EconomyService(
            GridModel gridModel,
            PlayerResourcesModel resourcesModel,
            BuildingConfigurationProvider configProvider,
            IPublisher<ResourcesUpdatedEventDTO> resourcesUpdatedPublisher)
        {
            this._gridModel = gridModel;
            this._resourcesModel = resourcesModel;
            this._configProvider = configProvider;
            this._resourcesUpdatedPublisher = resourcesUpdatedPublisher;
        }

        /// <summary>
        /// Вызывается VContainer'ом при старте. Запускает основной цикл производства ресурсов.
        /// </summary>
        public void Initialize()
        {
            this.ProductionLoop(this._cancellation.Token).Forget();
        }

        /// <summary>
        /// Основной цикл производства ресурсов.
        /// </summary>
        private async UniTaskVoid ProductionLoop(CancellationToken token)
        {
            while (!token.IsCancellationRequested)
            {
                try
                {
                    await UniTask.Delay(TimeSpan.FromSeconds(_tickIntervalSeconds), cancellationToken: token);
                    
                    this.ProcessProductionTick();
                }
                catch (OperationCanceledException)
                {
                    // Это нормальное исключение, когда мы останавливаем цикл по запросу.
                    break;
                }
            }
        }

        /// <summary>
        /// Обрабатывает один "тик", проходя по всем зданиям и начисляя ресурсы.
        /// </summary>
        private void ProcessProductionTick()
        {
            IEnumerable<BuildingInstanceModel> allBuildings = this._gridModel.GetAllBuildings();
            if (allBuildings == null) return;

            foreach (BuildingInstanceModel building in allBuildings)
            {
                BuildingTypeConfig config = this._configProvider.GetBuildingConfigByType(building.Type);
                if (config == null) continue;

                IReadOnlyDictionary<ResourceType, int> levelData = config.GetProduction(building.Level);
                if (levelData == null || levelData.Count == 0) continue;

                // Проверяем, хватает ли ресурсов на потребление (если оно есть)
                if (levelData.ContainsKey(ResourceType.Gold) && levelData[ResourceType.Gold] > 0)
                {
                    if (!this._resourcesModel.HasEnough(levelData))
                    {
                        continue; // Не производим, если не хватает ресурсов на содержание
                    }
                    
                    // Списываем ресурсы за потребление
                    this._resourcesModel.Spend(levelData);
                    // Уведомляем UI об изменении
                    foreach (KeyValuePair<ResourceType, int> consumed in levelData)
                    {
                        this._resourcesUpdatedPublisher.Publish(new ResourcesUpdatedEventDTO
                        {
                            ResourceType = consumed.Key,
                            NewAmount = this._resourcesModel.GetAmount(consumed.Key)
                        });
                    }
                }
                
                // Начисляем ресурсы за производство
                foreach (KeyValuePair<ResourceType, int> productionItem in levelData)
                {
                    this._resourcesModel.Add(productionItem.Key, productionItem.Value);
                    
                    // Уведомляем UI об изменении
                    this._resourcesUpdatedPublisher.Publish(new ResourcesUpdatedEventDTO
                    {
                        ResourceType = productionItem.Key,
                        NewAmount = this._resourcesModel.GetAmount(productionItem.Key)
                    });
                }
            }
        }

        /// <summary>
        /// Вызывается VContainer'ом при уничтожении. Останавливает цикл.
        /// </summary>
        public void Dispose()
        {
            this._cancellation.Cancel();
            this._cancellation.Dispose();
        }
    }
}