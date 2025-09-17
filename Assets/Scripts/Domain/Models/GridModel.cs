using System.Collections.Generic;
using UnityEngine;

namespace CityBuilder.Domain.Models
{
    /// <summary>
    /// Модель, хранящая состояние игровой сетки и всех зданий на ней.
    /// </summary>
    public class GridModel
    {


        // Быстрый поиск здания по его позиции
        private readonly Dictionary<Vector2Int, BuildingInstanceModel> _buildingsByPosition = new();

        /// <summary>
        /// Проверяет, занята ли указанная клетка.
        /// </summary>
        public bool IsCellOccupied(Vector2Int position)
        {
            return this._buildingsByPosition.ContainsKey(position);
        }

        /// <summary>
        /// Добавляет новое здание на сетку.
        /// </summary>
        /// <returns>True, если здание успешно добавлено.</returns>
        public bool AddBuilding(BuildingInstanceModel building)
        {
            if (this.IsCellOccupied(building.Position))
            {
                return false;
            }
            
            this._buildingsByPosition[building.Position] = building;
            return true;
        }

        /// <summary>
        /// Удаляет здание с сетки по его позиции
        /// </summary>
        /// <returns>Удаленная модель здания или null, если не найдено.</returns>
        public BuildingInstanceModel RemoveBuilding(Vector2Int position)
        {
            if (!this._buildingsByPosition.TryGetValue(position, out BuildingInstanceModel building))
            {
                return null;
            }

            this._buildingsByPosition.Remove(building.Position);
            return building;
        }

        /// <summary>
        /// Перемещает здание из одной клетки в другую.
        /// </summary>
        /// <returns>True, если перемещение удалось.</returns>
        public bool MoveBuilding(Vector2Int oldPosition, Vector2Int newPosition)
        {
            if (this.IsCellOccupied(newPosition))
            {
                return false;
            }
            
            if (!this._buildingsByPosition.TryGetValue(oldPosition, out BuildingInstanceModel building))
            {
                return false;
            }

            this._buildingsByPosition.Remove(oldPosition);
            building.Move(newPosition);
            this._buildingsByPosition[building.Position] = building;
            return true;
        }

        public BuildingInstanceModel GetBuildingAt(Vector2Int position)
        {
            this._buildingsByPosition.TryGetValue(position, out BuildingInstanceModel building);
            return building;
        }
        
        /// <summary>
        /// (Для тестов и сервисов) Получает список всех зданий.
        /// </summary>
        public IEnumerable<BuildingInstanceModel> GetAllBuildings()
        {
            return this._buildingsByPosition.Values;
        }

        public void Clear()
        {
            this._buildingsByPosition.Clear();
        }
    }
}