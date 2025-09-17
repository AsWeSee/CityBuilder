using MessagePipe;
using VContainer.Unity;
using System;
using CityBuilder.Domain.Models;
using CityBuilder.Domain.MessageDTO;

namespace CityBuilder.Application.UseCases
{
    public class RotateBuildingUseCase : IInitializable, IDisposable
    {
        private readonly PlacementStateModel _placementStateModel;
        private readonly ISubscriber<RotateBuildingRequestDTO> _subscriber;
        private IDisposable _disposable;

        public RotateBuildingUseCase(PlacementStateModel placementStateModel, ISubscriber<RotateBuildingRequestDTO> subscriber)
        {
            this._placementStateModel = placementStateModel;
            this._subscriber = subscriber;
        }

        public void Initialize()
        {
            this._disposable = this._subscriber.Subscribe(_ => HandleRotationRequest());
        }

        private void HandleRotationRequest()
        {
            // Вращаем здание только если мы в режиме постройки
            if (!this._placementStateModel.IsInPlacementMode) return;
            
            this._placementStateModel.PlacementRotationY = (this._placementStateModel.PlacementRotationY + 90f) % 360f;
        }

        public void Dispose() => this._disposable?.Dispose();
    }
}