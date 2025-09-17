using VContainer.Unity;
using System;
using MessagePipe;
using UnityEngine;
using CityBuilder.Domain.Models;
using CityBuilder.Presentation.UI.View;
using CityBuilder.Repositories;
using System.Collections.Generic;
using CityBuilder.Domain.MessageDTO;

namespace CityBuilder.Presentation.UI.Presenters
{
    public class BuildingTooltipPresenter : IInitializable, IDisposable
    {
        private readonly BuildingTooltipView _view;
        private readonly GridModel _gridModel;
        private readonly BuildingConfigurationProvider _configProvider;
        private readonly ISubscriber<PointerGridPositionChangedDTO> _pointerPosSubscriber;
        private IDisposable _disposable;

        public BuildingTooltipPresenter(
            BuildingTooltipView view, GridModel gridModel, BuildingConfigurationProvider configProvider,
            ISubscriber<PointerGridPositionChangedDTO> pointerPosSubscriber)
        {
            this._view = view;
            this._gridModel = gridModel;
            this._configProvider = configProvider;
            this._pointerPosSubscriber = pointerPosSubscriber;
        }

        public void Initialize()
        {
            this._disposable = this._pointerPosSubscriber.Subscribe(this.OnPointerGridPositionChanged);
        }

        private void OnPointerGridPositionChanged(PointerGridPositionChangedDTO dto)
        {
            if (!dto.IsInBounds)
            {
                this._view.Hide();
                return;
            }

            BuildingInstanceModel building = this._gridModel.GetBuildingAt(dto.GridPosition);
            if (building != null)
            {
                BuildingTypeConfig config = this._configProvider.GetBuildingConfigByType(building.Type);
                IReadOnlyDictionary<ResourceType, int> levelData = config.GetUpgradeCost(building.Level);
                
                Vector3 worldPos = new Vector3(dto.GridPosition.x + 0.5f, 1, dto.GridPosition.y + 0.5f);
                
                this._view.Show(worldPos, config.Name, levelData);
            }
            else
            {
                this._view.Hide();
            }
        }

        public void Dispose() => this._disposable?.Dispose();
    }
}