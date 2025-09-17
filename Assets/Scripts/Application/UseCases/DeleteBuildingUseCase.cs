using MessagePipe;
using VContainer.Unity;
using System;
using CityBuilder.Domain.Models;
using CityBuilder.Domain.MessageDTO;
using CityBuilder.Repositories;

namespace CityBuilder.Application.UseCases
{
    public class DeleteBuildingUseCase : IInitializable, IDisposable
    {
        private const float REFUND_PERCENTAGE = 0.5f; // 50%

        private readonly SelectionModel _selectionModel;
        private readonly GridModel _gridModel;
        private readonly ResourcesModel _resourcesModel;
        private readonly BuildingConfigurationProvider _configProvider;
        
        private readonly ISubscriber<DeleteSelectedBuildingRequestDTO> _deleteSubscriber;
        private readonly IPublisher<BuildingDeletedEventDTO> _deletedPublisher;
        private readonly IPublisher<ResourcesUpdatedEventDTO> _resourcesUpdatedPublisher;
        private IDisposable _disposable;

        public DeleteBuildingUseCase(
            SelectionModel selectionModel,
            GridModel gridModel,
            ResourcesModel resourcesModel,
            BuildingConfigurationProvider configProvider,
            ISubscriber<DeleteSelectedBuildingRequestDTO> deleteSubscriber,
            IPublisher<BuildingDeletedEventDTO> deletedPublisher,
            IPublisher<ResourcesUpdatedEventDTO> resourcesUpdatedPublisher
        ) {
            this._selectionModel = selectionModel;
            this._gridModel = gridModel;
            this._resourcesModel = resourcesModel;
            this._configProvider = configProvider;
            this._deleteSubscriber = deleteSubscriber;
            this._deletedPublisher = deletedPublisher;
            this._resourcesUpdatedPublisher = resourcesUpdatedPublisher;
        }

        public void Initialize()
        {
            this._disposable = this._deleteSubscriber.Subscribe(_ => HandleDeleteRequest());
        }

        private void HandleDeleteRequest()
        {
            if (!this._selectionModel.SelectedBuildingId.Value.HasValue) return;

            int buildingId = this._selectionModel.SelectedBuildingId.Value.Value;
            var buildingToRemove = this._gridModel.RemoveBuilding(buildingId);

            if (buildingToRemove != null)
            {
                // Возвращаем часть стоимости постройки
                var config = this._configProvider.GetBuildingTypeById(buildingToRemove.TypeId);
                var buildCost = config?.GetLevelData(1)?.BuildCost;
                if (buildCost != null)
                {
                    foreach (var resource in buildCost)
                    {
                        int refundAmount = (int)(resource.Value * REFUND_PERCENTAGE);
                        if (refundAmount > 0)
                        {
                            this._resourcesModel.Add(resource.Key, refundAmount);
                            _resourcesUpdatedPublisher.Publish(new ResourcesUpdatedEventDTO
                            {
                                Type = resource.Key,
                                NewAmount = this._resourcesModel.GetAmount(resource.Key)
                            });
                        }
                    }
                }

                // Снимаем выделение
                this._selectionModel.SelectedBuildingId.Value = null;
                
                this._deletedPublisher.Publish(new BuildingDeletedEventDTO { InstanceId = buildingId });
            }
        }
        
        public void Dispose() => this._disposable?.Dispose();
    }
}