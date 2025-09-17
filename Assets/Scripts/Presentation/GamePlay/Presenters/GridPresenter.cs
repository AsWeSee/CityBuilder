using VContainer.Unity;
using System;
using MessagePipe;
using CityBuilder.Domain.Models;
using CityBuilder.Domain.MessageDTO;
using CityBuilder.Presentation.GamePlay.Views;
using UnityEngine;
using R3;

namespace CityBuilder.Presentation.GamePlay.Presenters
{
    public class GridPresenter : IInitializable, IDisposable
    {
        private readonly GridView _view;
        private readonly GridModel _gridModel;
        private readonly PlacementStateModel _placementStateModel;
        private readonly SelectionModel _selectionModel;
        private readonly ISubscriber<PointerGridPositionChangedDTO> _pointerPosSubscriber;
        private readonly ISubscriber<BuildingPlacedEventDTO> _placedSubscriber;
        private IDisposable _disposables;

        public GridPresenter(
            GridView view,
            GridModel gridModel,
            PlacementStateModel placementStateModel,
            SelectionModel selectionModel,
            ISubscriber<PointerGridPositionChangedDTO> pointerPosSubscriber,
            ISubscriber<BuildingPlacedEventDTO> placedSubscriber)
        {
            this._view = view;
            this._gridModel = gridModel;
            this._placementStateModel = placementStateModel;
            this._selectionModel = selectionModel;
            this._pointerPosSubscriber = pointerPosSubscriber;
            this._placedSubscriber = placedSubscriber;
        }

        public void Initialize()
        {
            this._view.InitializeVisuals(); // Инициализируем визуальные элементы View
            DisposableBagBuilder bag = MessagePipe.DisposableBag.CreateBuilder();
            this._pointerPosSubscriber.Subscribe(this.OnPointerGridPositionChanged).AddTo(bag);
            this._placedSubscriber.Subscribe(this.OnBuildingPlaced).AddTo(bag);
            this._selectionModel.SelectedBuildingPosition.Subscribe(this.OnSelectionChanged).AddTo(bag);
            this._disposables = bag.Build();
            
            // Подписываемся на изменение выбора, чтобы обновить подсветку под зданием
        }

        private void OnSelectionChanged(Vector2Int? selectedBuildingPosition)
        {
            if (selectedBuildingPosition.HasValue)
            {
                BuildingInstanceModel building = this._gridModel.GetBuildingAt(selectedBuildingPosition.Value);
                if (building != null)
                {
                    // Показываем постоянную подсветку под выбранным зданием
                    this._view.ShowHighlight(GridView.HighlightType.Building, building.Position);
                }
            }
            else
            {
                // Если выбор снят, а мы не в режиме стройки, прячем все
                if (!this._placementStateModel.IsInPlacementMode)
                {
                    this._view.HideAllVisuals();
                }
            }
        }
        private void OnPointerGridPositionChanged(PointerGridPositionChangedDTO dto)
        {
            Vector2Int gridSize = this._view.GridSize;
            bool isInBounds = dto.GridPosition.x >= 0 && dto.GridPosition.x < gridSize.x &&
                              dto.GridPosition.y >= 0 && dto.GridPosition.y < gridSize.y;

            if (!isInBounds)
            {
                this._view.HideGhost();

                // Не прячем подсветку под выбранным зданием, если курсор ушел с поля
                if (this._selectionModel.SelectedBuildingPosition.Value == null) 
                    this._view.HideAllHighlights();

                return;
            }

            // Границы в порядке, показываем подсветку
            this._view.ShowHighlight(GridView.HighlightType.Select, dto.GridPosition);

            if (this._placementStateModel.IsInPlacementMode)
            {
                bool isCellOccupied = this._gridModel.IsCellOccupied(dto.GridPosition);
                
                if (isCellOccupied)
                {
                    this._view.ShowHighlight(GridView.HighlightType.FilledSpace, dto.GridPosition);
                }
                else
                {
                    float rotation = this._placementStateModel.PlacementRotationY;
                    this._view.ShowGhost(dto.GridPosition, rotation);
                    this._view.ShowHighlight(GridView.HighlightType.EmptySpace, dto.GridPosition);
                }
            }
            else
            {
                // Если мы не в режиме постройки, призрак должен быть скрыт
                this._view.HideGhost();
                
                // Если под курсором уже есть выбранное здание, ничего не делаем (под ним уже горит _cellBuildingHighlight)
                if (dto.GridPosition == this._selectionModel.SelectedBuildingPosition.Value)
                {
                    return;
                }

                // В остальных случаях показываем подсветку для выбора
                this._view.ShowHighlight(GridView.HighlightType.Select, dto.GridPosition);
            }
        }

        private void OnBuildingPlaced(BuildingPlacedEventDTO dto)
        {
            this._view.PlaceBuilding(dto.BuildingType, dto.Position);
        }


        public void Dispose() => this._disposables?.Dispose();
    }
}