using MessagePipe;
using UnityEngine;
using VContainer.Unity;
using CityBuilder.Domain.Models;
using CityBuilder.Domain.MessageDTO;
using CityBuilder.Repositories;
using System.Collections.Generic;
using System;
using CityBuilder.Infrastructure.MessagesDTO;

namespace CityBuilder.Application.UseCases
{
    public class PlaceBuildingUseCase : IInitializable, IDisposable
    {
        private readonly GridModel _gridModel;
        private readonly PlayerResourcesModel _resourcesModel;
        private readonly PlacementStateModel _placementStateModel;
        private readonly BuildingConfigurationProvider _buildingsConfigProvider;


        // --- Каналы для публикации событий ---
        private readonly IPublisher<BuildingPlacedEventDTO> _placedPublisher;
        private readonly IPublisher<BuildingPlacementFailedDTO> _failedPublisher;
        private readonly IPublisher<ResourcesUpdatedEventDTO> _resourcesUpdatedPublisher;

        // --- Каналы для подписки на запросы ---
        private readonly ISubscriber<PlacementRequestedDTO> _placementReqSubscriber;
        private readonly ISubscriber<PlacementCanceledDTO> _placementCancelSubscriber;
        private IDisposable _disposables;

        public PlaceBuildingUseCase(
            GridModel gridModel, 
        PlayerResourcesModel resourcesModel, 
        PlacementStateModel placementStateModel, 
        BuildingConfigurationProvider buildingsConfigProvider, 
        ISubscriber<PlacementRequestedDTO> placementReqSubscriber,
        ISubscriber<PlacementCanceledDTO> placementCancelSubscriber,
        IPublisher<BuildingPlacedEventDTO> placedPublisher,
        IPublisher<BuildingPlacementFailedDTO> failedPublisher,
        IPublisher<ResourcesUpdatedEventDTO> resourcesUpdatedPublisher)
        {
            this._placementReqSubscriber = placementReqSubscriber;
            this._placementCancelSubscriber = placementCancelSubscriber;
            this._placedPublisher = placedPublisher;
            this._failedPublisher = failedPublisher;
            this._resourcesUpdatedPublisher = resourcesUpdatedPublisher;

            this._placementStateModel = placementStateModel;
            this._resourcesModel = resourcesModel;
            this._buildingsConfigProvider = buildingsConfigProvider;
            this._gridModel = gridModel;
        }
        
        public void Initialize()
        {
            DisposableBagBuilder bag = DisposableBag.CreateBuilder();
            this._placementReqSubscriber.Subscribe(this.HandlePlacementRequest).AddTo(bag);
            this._placementCancelSubscriber.Subscribe(_ => this.HandlePlacementCancel()).AddTo(bag);
            this._disposables = bag.Build();
        }

        private void HandlePlacementRequest(PlacementRequestedDTO dto)
        {
            if (!this._placementStateModel.IsInPlacementMode) return;

            BuildingType buildingType = this._placementStateModel.SelectedBuildingType.Value;
            BuildingTypeConfig config = this._buildingsConfigProvider.GetBuildingConfigByType(buildingType);
            if (config == null) return;


            if (this._gridModel.IsCellOccupied(dto.GridPosition))
            {
                this._failedPublisher.Publish(new BuildingPlacementFailedDTO { Reason = FailureReason.CellIsOccupied });
                return;
            }


            IReadOnlyDictionary<ResourceType, int> cost = config.GetBuildCost();
            if (!this._resourcesModel.HasEnough(cost))
            {
                this._failedPublisher.Publish(new BuildingPlacementFailedDTO { Reason = FailureReason.NotEnoughResources });
                return;
            }


            this._resourcesModel.Spend(cost);
            
            foreach (KeyValuePair<ResourceType, int> resourceCost in cost)
            {
                this._resourcesUpdatedPublisher.Publish(new ResourcesUpdatedEventDTO
                {
                    ResourceType = resourceCost.Key,
                    NewAmount = this._resourcesModel.GetAmount(resourceCost.Key)
                });
            }


            // TODO: Нужен сервис для генерации уникальных ID
            int newInstanceId = UnityEngine.Random.Range(1000, 100000);
            BuildingInstanceModel newBuilding = new BuildingInstanceModel(newInstanceId, buildingType, dto.GridPosition);
            this._gridModel.AddBuilding(newBuilding);

            Debug.Log($"Building {config.Name} placed at {dto.GridPosition}");


            this._placedPublisher.Publish(new BuildingPlacedEventDTO
            {
                InstanceId = newInstanceId,
                BuildingType = buildingType,
                Position = dto.GridPosition
            });

            this._placementStateModel.SelectedBuildingType = null;

        }



        private void HandlePlacementCancel()
        {
            if (!this._placementStateModel.IsInPlacementMode) return;
            
            this._placementStateModel.SelectedBuildingType = null;
        }
        public void Dispose() => this._disposables?.Dispose();
    }
}