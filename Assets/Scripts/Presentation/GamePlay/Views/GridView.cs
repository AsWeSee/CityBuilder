using UnityEngine;
using System.Collections.Generic;
using System;
using CityBuilder.Domain.Models;
using System.Linq;

namespace CityBuilder.Presentation.GamePlay.Views
{

    [Serializable]
    public class BuildingPrefabMapping
    {
        public BuildingType BuildingType;
        public GameObject Prefab;
    }
    public class GridView : MonoBehaviour
    {
        [Header("Grid Settings")]
        [SerializeField] private Vector2Int _gridSize = new(32, 32);
        public Vector2Int GridSize => this._gridSize;
        [SerializeField] private float _cellSize = 1f;

        [Header("Подсветка Клеток")]
        [SerializeField] private GameObject _buildingGhostPrefab;
        [SerializeField] private GameObject _cellSelectHighlightPrefab;
        [SerializeField] private GameObject _cellBuildingHighlightPrefab;
        [SerializeField] private GameObject _cellEmptySpaceHighlightPrefab;
        [SerializeField] private GameObject _cellFilledSpaceHighlightPrefab;

        [Header("Prefabs")]
        [SerializeField] private List<BuildingPrefabMapping> _buildingPrefabs;

        private Dictionary<BuildingType, GameObject> _buildingInstances = new();


        // --- Экземпляры объектов ---
        private GameObject _ghostInstance;
        private GameObject _selectInstance;
        private GameObject _buildingInstance;
        private GameObject _emptySpaceInstance;
        private GameObject _filledSpaceInstance;

        // Словарь для всех подсветок для удобного управления
        private Dictionary<HighlightType, GameObject> _highlights;

        // Перечисление для команд от Presenter'а
        public enum HighlightType { None, Select, Building, EmptySpace, FilledSpace }

        public void InitializeVisuals()
        {
            // Создаем все экземпляры и сразу их прячем
            this._ghostInstance = Instantiate(this._buildingGhostPrefab);
            this._selectInstance = Instantiate(this._cellSelectHighlightPrefab);
            this._buildingInstance = Instantiate(this._cellBuildingHighlightPrefab);
            this._emptySpaceInstance = Instantiate(this._cellEmptySpaceHighlightPrefab);
            this._filledSpaceInstance = Instantiate(this._cellFilledSpaceHighlightPrefab);

            this._highlights = new Dictionary<HighlightType, GameObject>
        {
            { HighlightType.Select, this._selectInstance },
            { HighlightType.Building, this._buildingInstance },
            { HighlightType.EmptySpace, this._emptySpaceInstance },
            { HighlightType.FilledSpace, this._filledSpaceInstance }
        };

            this.HideAllHighlights();
            this.HideGhost();

        }

        /// <summary>
        /// Показывает один конкретный тип подсветки в указанной позиции, скрывая остальные.
        /// </summary>
        public void ShowHighlight(HighlightType type, Vector2Int gridPosition)
        {
            this.HideAllHighlights(); // Сначала все прячем для чистоты

            if (type != HighlightType.None && this._highlights.TryGetValue(type, out GameObject highlightInstance))
            {
                highlightInstance.SetActive(true);
                highlightInstance.transform.position = this.GetWorldPosition(gridPosition);
            }
        }

        public void HideAllHighlights()
        {
            foreach (GameObject highlight in this._highlights.Values)
            {
                highlight.SetActive(false);
            }
        }

        public void ShowGhost(Vector2Int gridPosition /*, float rotationY */)
        {
            this._ghostInstance.SetActive(true);
            this._ghostInstance.transform.position = this.GetWorldPosition(gridPosition);
            // _ghostInstance.transform.rotation = Quaternion.Euler(0, rotationY, 0);
        }

        public void HideGhost()
        {
            this._ghostInstance.SetActive(false);
        }

        public void HideAllVisuals()
        {
            this.HideAllHighlights();
            this.HideGhost();
        }

        public void PlaceBuilding(BuildingType buildingType, Vector2Int position)
        {
            BuildingPrefabMapping prefabMapping = this._buildingPrefabs.FirstOrDefault(mapping => mapping.BuildingType == buildingType);
            if (prefabMapping == null)
            {
                Debug.LogError($"Prefab not found for Building Type: {buildingType}");
                return;
            }

            Vector3 worldPosition = this.GetWorldPosition(position);
            GameObject newInstance = Instantiate(prefabMapping.Prefab, worldPosition, Quaternion.identity, this.transform);
            this._buildingInstances[buildingType] = newInstance;

            Debug.Log($"[GridView] Instantiated prefab for building instance {buildingType}");
        }
        private Vector3 GetWorldPosition(Vector2Int gridPosition)
        {
            return new Vector3(gridPosition.x, 0, gridPosition.y) * this._cellSize + new Vector3(this._cellSize, 0, this._cellSize) * 0.5f;
        }
    }
}