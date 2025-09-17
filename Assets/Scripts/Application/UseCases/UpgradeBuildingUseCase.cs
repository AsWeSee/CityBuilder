using MessagePipe;
using VContainer.Unity;
using System;
using CityBuilder.Domain.Models;
using CityBuilder.Domain.MessageDTO;
using CityBuilder.Repositories;
using System.Collections.Generic;

namespace CityBuilder.Application.UseCases
{
    public class UpgradeBuildingUseCase : IInitializable, IDisposable
    {
        private readonly SelectionModel _selectionModel;
        private readonly GridModel _gridModel;
        private readonly PlayerResourcesModel _resourcesModel;
        private readonly BuildingConfigurationProvider _configProvider;
        
        private readonly ISubscriber<UpgradeSelectedBuildingRequestDTO> _upgradeSubscriber;
        private readonly IPublisher<BuildingUpgradedEventDTO> _upgradedPublisher;
        private readonly IPublisher<Infrastructure.MessagesDTO.ResourcesUpdatedEventDTO> _resourcesUpdatedPublisher;
        private readonly IPublisher<BuildingUpgradeFailedDTO> _failedPublisher;
        private IDisposable _disposable;

        public UpgradeBuildingUseCase(
            SelectionModel selectionModel,
            GridModel gridModel,
            PlayerResourcesModel resourcesModel,
            BuildingConfigurationProvider configProvider,
            ISubscriber<UpgradeSelectedBuildingRequestDTO> upgradeSubscriber,
            IPublisher<BuildingUpgradedEventDTO> upgradedPublisher,
            IPublisher<Infrastructure.MessagesDTO.ResourcesUpdatedEventDTO> resourcesUpdatedPublisher,
            IPublisher<BuildingUpgradeFailedDTO> failedPublisher
        ) {
            this._selectionModel = selectionModel;
            this._gridModel = gridModel;
            this._resourcesModel = resourcesModel;
            this._configProvider = configProvider;
            this._upgradeSubscriber = upgradeSubscriber;
            this._upgradedPublisher = upgradedPublisher;
            this._resourcesUpdatedPublisher = resourcesUpdatedPublisher;
            this._failedPublisher = failedPublisher;
        }

        public void Initialize()
        {
            this._disposable = this._upgradeSubscriber.Subscribe(_ => this.HandleUpgradeRequest());
        }

        private void HandleUpgradeRequest()
        {
            if (!this._selectionModel.SelectedBuildingPosition.Value.HasValue)
            {
                this._failedPublisher.Publish(new BuildingUpgradeFailedDTO { Reason = UpgradeFailureReason.NotSelected });
                return;
            }

            BuildingInstanceModel building = this._gridModel.GetBuildingAt(this._selectionModel.SelectedBuildingPosition.Value.Value);
            BuildingTypeConfig config = this._configProvider.GetBuildingConfigByType(building.Type);

            if (building.Level >= config.MaxLevel)
            {
                this._failedPublisher.Publish(new BuildingUpgradeFailedDTO { Reason = UpgradeFailureReason.MaxLevelReached });
                return;
            }

            IReadOnlyDictionary<ResourceType, int> nextLevelData = config.GetUpgradeCost(building.Level + 1);

            if (!this._resourcesModel.HasEnough(nextLevelData))
            {
                this._failedPublisher.Publish(new BuildingUpgradeFailedDTO { Reason = UpgradeFailureReason.NotEnoughResources });
                return;
            }
            
            // --- Все проверки пройдены, улучшаем ---
        
            this._resourcesModel.Spend(nextLevelData);
            foreach (KeyValuePair<ResourceType, int> resourceCost in nextLevelData)
            {
                this._resourcesUpdatedPublisher.Publish(new Infrastructure.MessagesDTO.ResourcesUpdatedEventDTO
                {
                    ResourceType = resourceCost.Key,
                    NewAmount = this._resourcesModel.GetAmount(resourceCost.Key)
                });
            }
            
            building.LevelUp();
            
            this._upgradedPublisher.Publish(new BuildingUpgradedEventDTO
            {
                InstanceId = building.InstanceId,
                NewLevel = building.Level
            });
        }
        
        public void Dispose() => this._disposable?.Dispose();
    }
}