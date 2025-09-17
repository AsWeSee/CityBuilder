using MessagePipe;
using CityBuilder.Domain.Models;
using CityBuilder.Domain.MessageDTO; // Где лежит SelectBuildingToPlaceRequestDTO
using VContainer.Unity;
using UnityEngine;

namespace CityBuilder.Application.UseCases
{
    public class EnterPlacementModeUseCase : IInitializable
    {
        private readonly PlacementStateModel _placementStateModel;
        private readonly ISubscriber<SelectBuildingToPlaceRequestDTO> _subscriber;

        public EnterPlacementModeUseCase(PlacementStateModel placementStateModel, ISubscriber<SelectBuildingToPlaceRequestDTO> subscriber)
        {
            this._placementStateModel = placementStateModel;
            this._subscriber = subscriber;
        }

        public void Initialize()
        {
            this._subscriber.Subscribe(this.HandleRequest);
        }
        
        private void HandleRequest(SelectBuildingToPlaceRequestDTO dto)
        {
            this._placementStateModel.SelectedBuildingType = dto.BuildingType;
            Debug.Log($"Entered placement mode for Building ID: {dto.BuildingType}");
        }
    }
}