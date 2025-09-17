using VContainer.Unity;
using System;
using MessagePipe;
using UnityEngine;
using R3;
using CityBuilder.Domain.Models;
using CityBuilder.Domain.MessageDTO;
using CityBuilder.Presentation.UI.View;
using CityBuilder.Repositories;
using System.Collections.Generic;

namespace CityBuilder.Presentation.UI.Presenters
{
    public class BuildingContextMenuPresenter : IInitializable, IDisposable
    {
        private readonly BuildingContextMenuView _view;
        private readonly SelectionModel _selectionModel;
        private readonly GridModel _gridModel;


        private readonly BuildingConfigurationProvider _configProvider;
        private readonly IPublisher<UpgradeSelectedBuildingRequestDTO> _upgradePublisher;
        private readonly IPublisher<MoveSelectedBuildingRequestDTO> _movePublisher;
        private readonly IPublisher<DeleteSelectedBuildingRequestDTO> _deletePublisher;

        private IDisposable _disposables;

        public BuildingContextMenuPresenter(
            BuildingContextMenuView view, SelectionModel selectionModel, GridModel gridModel,
            BuildingConfigurationProvider configProvider,
            IPublisher<UpgradeSelectedBuildingRequestDTO> upgradePublisher,
            IPublisher<MoveSelectedBuildingRequestDTO> movePublisher,
            IPublisher<DeleteSelectedBuildingRequestDTO> deletePublisher)
        {
            this._view = view;
            this._selectionModel = selectionModel;
            this._gridModel = gridModel;
            this._configProvider = configProvider;
            this._upgradePublisher = upgradePublisher;
            this._movePublisher = movePublisher;
            this._deletePublisher = deletePublisher;
        }

        public void Initialize()
        {
            DisposableBagBuilder bag = MessagePipe.DisposableBag.CreateBuilder();

            // 1. Реагируем на изменение выбранного здания
            this._selectionModel.SelectedBuildingPosition
                .Subscribe(this.OnSelectionChanged)
                .AddTo(bag);

            // 2. Реагируем на нажатия кнопок в View
            this._view.OnUpgradeClicked += () => this._upgradePublisher.Publish(new UpgradeSelectedBuildingRequestDTO());
            this._view.OnMoveClicked += () => this._movePublisher.Publish(new MoveSelectedBuildingRequestDTO());
            this._view.OnDeleteClicked += () => 
            {
                Debug.Log("BuildingContextMenuPresenter: Delete button event received, publishing DeleteSelectedBuildingRequestDTO");
                this._deletePublisher.Publish(new DeleteSelectedBuildingRequestDTO());
            };

            this._disposables = bag.Build();
        }

        private void OnSelectionChanged(Vector2Int? selectedPosition)
        {
            if (selectedPosition == null)
            {
                this._view.Hide();
                return;
            }

            BuildingInstanceModel building = this._gridModel.GetBuildingAt(selectedPosition.Value);

            if (building == null)
            {
                this._view.Hide();
                return;
            }
            BuildingTypeConfig config = this._configProvider.GetBuildingConfigByType(building.Type);
            if (config == null) return;

            // --- Собираем данные для View ---
            bool canUpgrade = building.Level < config.MaxLevel;
            IReadOnlyDictionary<ResourceType, int> upgradeCost = null;
            IReadOnlyDictionary<ResourceType, int> nextLevelProduction = null;

            if (canUpgrade)
            {
                IReadOnlyDictionary<ResourceType, int> nextLevelData = config.GetUpgradeCost(building.Level + 1);
                if (nextLevelData != null)
                {
                    upgradeCost = nextLevelData;
                    nextLevelProduction = config.GetProduction(building.Level + 1);
                }
            }

            // --- Вызываем View с полным набором данных ---
            Vector3 worldPos = new Vector3(building.Position.x + 0.5f, 1, building.Position.y + 0.5f);

            this._view.Show(worldPos, canUpgrade, upgradeCost, nextLevelProduction);

        }

        public void Dispose() => this._disposables?.Dispose();
    }
}