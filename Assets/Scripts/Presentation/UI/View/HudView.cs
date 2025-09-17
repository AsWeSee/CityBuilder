using System;
using UnityEngine;
using UnityEngine.UIElements;
using CityBuilder.Domain.Models; // Для ResourceType

namespace CityBuilder.Presentation.Views
{
    [RequireComponent(typeof(UIDocument))]
    public class HudView : MonoBehaviour
    {
        // События, на которые подпишется Presenter
        public event Action<BuildingType> OnBuildButtonClicked; // int - TypeId здания

        // Ссылки на элементы UI
        private Label _goldLabel;
        private Label _woodLabel;
        private Label _grainLabel;
        // ... другие ресурсы

        private void Awake()
        {
            VisualElement root = this.GetComponent<UIDocument>().rootVisualElement;

            // Находим элементы по их именам из UXML
            this._goldLabel = root.Q<Label>("gold-label");
            this._woodLabel = root.Q<Label>("wood-label");
            this._grainLabel = root.Q<Label>("grain-label");

            // Находим кнопки и подписываемся на их нажатия
            // Передаем в событие TypeId здания, который мы "зашиваем" здесь
            root.Q<Button>("house-button").clicked += () => this.OnBuildButtonClicked?.Invoke(BuildingType.House); // Предположим, у House TypeId = 1
            root.Q<Button>("farm-button").clicked += () => this.OnBuildButtonClicked?.Invoke(BuildingType.Farm);  // У Farm TypeId = 2
            root.Q<Button>("mine-button").clicked += () => this.OnBuildButtonClicked?.Invoke(BuildingType.Mine);  // У Mine TypeId = 3
        }

        /// <summary>
        /// Пассивный метод, который Presenter будет вызывать для обновления текста.
        /// </summary>
        public void UpdateResourceAmount(ResourceType type, int amount)
        {
            // Debug.Log($"HudView: UpdateResourceAmount {type} {amount}");
            string text = $"{amount}";
            switch (type)
            {
                case ResourceType.Gold:
                    this._goldLabel.text = text;
                    break;
                case ResourceType.Wood:
                    this._woodLabel.text = text;
                    break;
                case ResourceType.Grain:
                    this._grainLabel.text = text;
                    break;
            }
        }
    }
}

namespace CityBuilder.Presentation.MessagesDTO
{
    /// <summary>
    /// Сообщение от Presenter к UseCase: "Игрок хочет начать постройку этого здания".
    /// </summary>
    public struct SelectBuildingToPlaceRequestDTO
    {
        public BuildingType BuildingType;
    }
}