using System;
using System.Collections.Generic;
using System.Linq;
using CityBuilder.Domain.Models;
using UnityEngine;
using UnityEngine.UIElements;

namespace CityBuilder.Presentation.UI.View
{
    [RequireComponent(typeof(UIDocument))]
    public class BuildingContextMenuView : MonoBehaviour
    {
        public event Action OnUpgradeClicked;
        public event Action OnMoveClicked;
        public event Action OnDeleteClicked;

        private VisualElement _root;
        private Button _upgradeButton;
        private VisualElement _upgradeInfo;
        private Label _upgradeCostLabel;
        private VisualElement _nextLevelProductionList;

        private void Awake()
        {
            this._root = this.GetComponent<UIDocument>().rootVisualElement.Q("context-menu-root");
            Debug.Log($"BuildingContextMenuView Awake - Root found: {this._root != null}");

            Button upgradeButton = this._root.Q<Button>("upgrade-button");
            Button moveButton = this._root.Q<Button>("move-button");
            Button deleteButton = this._root.Q<Button>("delete-button");
            
            Debug.Log($"Buttons found - Upgrade: {upgradeButton != null}, Move: {moveButton != null}, Delete: {deleteButton != null}");

            if (upgradeButton != null)
                upgradeButton.clicked += () => OnUpgradeClicked?.Invoke();
            
            this._upgradeInfo = this._root.Q<VisualElement>("upgrade-info");
            this._upgradeCostLabel = this._root.Q<Label>("upgrade-cost-label");
            this._nextLevelProductionList = this._root.Q<VisualElement>("next-level-production-list");

            if (moveButton != null)
                moveButton.clicked += () => OnMoveClicked?.Invoke();
            
            if (deleteButton != null)
            {
                deleteButton.clicked += () => 
                {
                    Debug.Log("Delete button clicked!");
                    OnDeleteClicked?.Invoke();
                };
            }

            this.Hide(); // Скрываем меню по умолчанию
        }
        public void Show(
               Vector3 worldPosition,
               bool canUpgrade,
               IReadOnlyDictionary<ResourceType, int> upgradeCost,
               IReadOnlyDictionary<ResourceType, int> nextLevelProduction)
        {
            this._root.style.display = DisplayStyle.Flex;
            this.transform.position = worldPosition;

            // Показываем/скрываем кнопку и информацию об улучшении
            this._upgradeButton.style.display = canUpgrade ? DisplayStyle.Flex : DisplayStyle.None;
            this._upgradeInfo.style.display = canUpgrade ? DisplayStyle.Flex : DisplayStyle.None;

            if (canUpgrade)
            {
                // Формируем строку стоимости
                string costText = "Стоимость: ";
                if (upgradeCost != null)
                    costText += string.Join(", ", upgradeCost.Select(kvp => $"{kvp.Key} {kvp.Value}"));
                this._upgradeCostLabel.text = costText;

                // Заполняем список производства на след. уровне
                this._nextLevelProductionList.Clear();
                if (nextLevelProduction != null)
                {
                    foreach (KeyValuePair<ResourceType, int> res in nextLevelProduction)
                    {
                        this._nextLevelProductionList.Add(new Label($"+{res.Value} {res.Key}/tick"));
                    }
                }
            }
        }

        public void Hide()
        {
            this._root.style.display = DisplayStyle.None;
        }
    }
}