using MessagePipe;
using VContainer.Unity;
using System;
using CityBuilder.Domain.Models;
using CityBuilder.Domain.MessageDTO;
using CityBuilder.Repositories;

namespace CityBuilder.Application.UseCases
{
    public class UpgradeBuildingUseCase : IInitializable, IDisposable
    {
        private readonly SelectionModel _selectionModel;
        private readonly GridModel _gridModel;
        private readonly ResourcesModel _resourcesModel;
        private readonly BuildingConfigurationProvider _configProvider;
        
        private readonly ISubscriber<UpgradeSelectedBuildingRequestDTO> _upgradeSubscriber;
        private readonly IPublisher<BuildingUpgradedEventDTO> _upgradedPublisher;
        private readonly IPublisher<ResourcesUpdatedEventDTO> _resourcesUpdatedPublisher;
        private readonly IPublisher<BuildingUpgradeFailedDTO> _failedPublisher;
        private IDisposable _disposable;

        public UpgradeBuildingUseCase(
            SelectionModel selectionModel,
            GridModel gridModel,
            ResourcesModel resourcesModel,
            BuildingConfigurationProvider configProvider,
            ISubscriber<UpgradeSelectedBuildingRequestDTO> upgradeSubscriber,
            IPublisher<BuildingUpgradedEventDTO> upgradedPublisher,
            IPublisher<ResourcesUpdatedEventDTO> resourcesUpdatedPublisher,
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
            this._disposable = this._upgradeSubscriber.Subscribe(_ => HandleUpgradeRequest());
        }

        private void HandleUpgradeRequest()
        {
            if (!this._selectionModel.SelectedBuildingId.Value.HasValue)
            {
                this._failedPublisher.Publish(new BuildingUpgradeFailedDTO { Reason = UpgradeFailureReason.NotSelected });
                return;
            }

            var building = this._gridModel.GetBuildingById(this._selectionModel.SelectedBuildingId.Value.Value);
            var config = this._configProvider.GetBuildingTypeById(building.TypeId);

            if (building.Level >= config.MaxLevel)
            {
                this._failedPublisher.Publish(new BuildingUpgradeFailedDTO { Reason = UpgradeFailureReason.MaxLevelReached });
                return;
            }

            var nextLevelData = config.GetLevelData(building.Level + 1);
            var upgradeCost = nextLevelData.UpgradeCost;

            if (!this._resourcesModel.HasEnough(upgradeCost))
            {
                this._failedPublisher.Publish(new BuildingUpgradeFailedDTO { Reason = UpgradeFailureReason.NotEnoughResources });
                return;
            }
            
            // --- Все проверки пройдены, улучшаем ---
        
            this._resourcesModel.Spend(upgradeCost);
            foreach (var resourceCost in upgradeCost)
            {
                this._resourcesUpdatedPublisher.Publish(new ResourcesUpdatedEventDTO
                {
                    Type = resourceCost.Key,
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