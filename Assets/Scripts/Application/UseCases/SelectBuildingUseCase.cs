using MessagePipe;
using VContainer.Unity;
using System;
using CityBuilder.Domain.Models;
using CityBuilder.Domain.MessageDTO;
namespace CityBuilder.Application.UseCases
{
    public class SelectBuildingUseCase : IInitializable, IDisposable
    {
        private readonly GridModel _gridModel;
        private readonly SelectionModel _selectionModel;
        private readonly ISubscriber<PlacementRequestedDTO> _clickSubscriber;
        private IDisposable _disposable;
        public SelectBuildingUseCase(
                GridModel gridModel,
                SelectionModel selectionModel,
                ISubscriber<PlacementRequestedDTO> clickSubscriber)
        {
            this._gridModel = gridModel;
            this._selectionModel = selectionModel;
            this._clickSubscriber = clickSubscriber;
        }

        public void Initialize()
        {
            this._disposable = this._clickSubscriber.Subscribe(this.HandleGridClick);
        }

        private void HandleGridClick(PlacementRequestedDTO dto)
        {
            BuildingInstanceModel building = this._gridModel.GetBuildingAt(dto.GridPosition);

            if (building != null)
            {
                // Если на клетке есть здание, выбираем его
                this._selectionModel.SelectedBuildingPosition.Value = building.Position;
            }
            else
            {
                // Если клетка пуста, снимаем выделение
                this._selectionModel.SelectedBuildingPosition.Value = null;
            }
        }

        public void Dispose() => this._disposable?.Dispose();
    }
}