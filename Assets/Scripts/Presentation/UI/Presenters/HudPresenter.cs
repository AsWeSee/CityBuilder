using System;
using UnityEngine;
using VContainer.Unity;
using MessagePipe;
using CityBuilder.Domain.MessageDTO;
using CityBuilder.Domain.Models;
using CityBuilder.Presentation.Views;
using CityBuilder.Infrastructure.MessagesDTO;

namespace CityBuilder.Presentation.Presenters
{
    public class HudPresenter : IInitializable, IDisposable, IStartable
    {
        private readonly HudView _view;
        private readonly PlayerResourcesModel _resourcesModel;
        private readonly IPublisher<SelectBuildingToPlaceRequestDTO> _selectBuildingPublisher;
        private readonly ISubscriber<ResourcesUpdatedEventDTO> _resourcesUpdatedSubscriber;
        private IDisposable _disposable;

        public HudPresenter(
            HudView view,
            PlayerResourcesModel resourcesModel,
            IPublisher<SelectBuildingToPlaceRequestDTO> selectBuildingPublisher,
            ISubscriber<ResourcesUpdatedEventDTO> resourcesUpdatedSubscriber)
        {
            this._view = view;
            this._resourcesModel = resourcesModel;
            this._selectBuildingPublisher = selectBuildingPublisher;
            this._resourcesUpdatedSubscriber = resourcesUpdatedSubscriber;
        }

        public void Initialize()
        {
            // События от View
            this._view.OnBuildButtonClicked += this.HandleBuildButtonClick;

            // События от игры
            DisposableBagBuilder bag = DisposableBag.CreateBuilder();
            this._resourcesUpdatedSubscriber.Subscribe(this.HandleResourcesUpdated).AddTo(bag);
            this._disposable = bag.Build();
        }

        public void Start()
        {
            this.FillResourceDisplay();
        }

        private void FillResourceDisplay()
        {
            foreach (ResourceType resourceType in Enum.GetValues(typeof(ResourceType)))
            {
                this._view.UpdateResourceAmount(resourceType, this._resourcesModel.GetAmount(resourceType));
            }
        }
        
        
        private void HandleBuildButtonClick(BuildingType buildingType)
        {
            Debug.Log($"UI: Player wants to build building with ID: {buildingType}");
            // Отправляем сообщение в систему, что игрок выбрал здание для постройки
            this._selectBuildingPublisher.Publish(new SelectBuildingToPlaceRequestDTO { BuildingType = buildingType });
        }
        
        
        private void HandleResourcesUpdated(ResourcesUpdatedEventDTO dto)
        {
            this._view.UpdateResourceAmount(dto.ResourceType, dto.NewAmount);
        }

        public void Dispose()
        {
            this._view.OnBuildButtonClicked -= this.HandleBuildButtonClick;
            this._disposable?.Dispose();
        }
    }
}