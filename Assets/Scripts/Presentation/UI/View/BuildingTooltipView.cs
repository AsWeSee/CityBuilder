using UnityEngine;
using UnityEngine.UIElements;
using System.Collections.Generic;
using CityBuilder.Domain.Models; // Для ResourceType

namespace CityBuilder.Presentation.UI.View
{
    [RequireComponent(typeof(UIDocument))]
    public class BuildingTooltipView : MonoBehaviour
    {
        private VisualElement _root;
        private Label _buildingNameLabel;
        private VisualElement _productionList;

        // (Опционально) Префаб для строки с ресурсом
        // [SerializeField] private VisualTreeAsset _resourceLineTemplate;

        private void Awake()
        {
            this._root = this.GetComponent<UIDocument>().rootVisualElement.Q("tooltip-root");
            // this. _buildingNameLabel = this._root.Q<Label>("building-name-label");
            // this._productionList = this._root.Q("production-list");
            this.Hide();
        }

        public void Show(Vector3 worldPosition, string buildingName, IReadOnlyDictionary<ResourceType, int> production)
        {
            this._root.style.display = DisplayStyle.Flex;
            this.transform.position = worldPosition;

            // this._buildingNameLabel.text = buildingName;
            
            // this._productionList.Clear(); // Очищаем старые данные
            // if (production != null)
            // {
            //     foreach (KeyValuePair<ResourceType, int> res in production)
            //     {
            //         // Динамически создаем Label для каждой строки
            //         Label lineLabel = new Label($"{res.Key}: +{res.Value}/tick");
            //         this._productionList.Add(lineLabel);
            //     }
            // }
        }

        public void Hide()
        {
            this._root.style.display = DisplayStyle.None;
        }
    }
}   