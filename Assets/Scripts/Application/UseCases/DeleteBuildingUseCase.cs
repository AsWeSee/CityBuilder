using MessagePipe;
using VContainer.Unity;
using System;
using CityBuilder.Domain.Models;
using CityBuilder.Domain.MessageDTO;
using CityBuilder.Repositories;
using System.Collections.Generic;
using UnityEngine;

namespace CityBuilder.Application.UseCases
{
    public class DeleteBuildingUseCase : IInitializable, IDisposable
    {
        private const float _refund_percentage = 0.5f; // 50%

        private readonly SelectionModel _selectionModel;
        private readonly GridModel _gridModel;
        private readonly PlayerResourcesModel _resourcesModel;
        private readonly BuildingConfigurationProvider _configProvider;
        
        private readonly ISubscriber<DeleteSelectedBuildingRequestDTO> _deleteSubscriber;
        private readonly IPublisher<BuildingDeletedEventDTO> _deletedPublisher;
        private readonly IPublisher<Infrastructure.MessagesDTO.ResourcesUpdatedEventDTO> _resourcesUpdatedPublisher;
        private IDisposable _disposable;

        public DeleteBuildingUseCase(
            SelectionModel selectionModel,
            GridModel gridModel,
            PlayerResourcesModel resourcesModel,
            BuildingConfigurationProvider configProvider,
            ISubscriber<DeleteSelectedBuildingRequestDTO> deleteSubscriber,
            IPublisher<BuildingDeletedEventDTO> deletedPublisher,
            IPublisher<Infrastructure.MessagesDTO.ResourcesUpdatedEventDTO> resourcesUpdatedPublisher
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
            this._disposable = this._deleteSubscriber.Subscribe(_ => this.HandleDeleteRequest());
        }

        private void HandleDeleteRequest()
        {
            if (!this._selectionModel.SelectedBuildingPosition.Value.HasValue) { return; }

            Vector2Int buildingPosition = this._selectionModel.SelectedBuildingPosition.Value.Value;
            BuildingInstanceModel buildingToRemove = this._gridModel.RemoveBuilding(buildingPosition);

            if (buildingToRemove != null)
            {
                // Возвращаем часть стоимости постройки
                BuildingTypeConfig config = this._configProvider.GetBuildingConfigByType(buildingToRemove.Type);
                IReadOnlyDictionary<ResourceType, int> buildCost = config?.GetBuildCost();
                if (buildCost != null)
                {
                    foreach (KeyValuePair<ResourceType, int> resource in buildCost)
                    {
                        int refundAmount = (int)(resource.Value * _refund_percentage);
                        if (refundAmount > 0)
                        {
                            this._resourcesModel.Add(resource.Key, refundAmount);
                            this._resourcesUpdatedPublisher.Publish(new Infrastructure.MessagesDTO.ResourcesUpdatedEventDTO
                            {
                                ResourceType = resource.Key,
                                NewAmount = this._resourcesModel.GetAmount(resource.Key)
                            });
                        }
                    }
                }

                // Снимаем выделение
                this._selectionModel.SelectedBuildingPosition.Value = null;
                
                this._deletedPublisher.Publish(new BuildingDeletedEventDTO { InstanceId = buildingToRemove.InstanceId });
            }
        }
        
        public void Dispose() => this._disposable?.Dispose();
    }
}