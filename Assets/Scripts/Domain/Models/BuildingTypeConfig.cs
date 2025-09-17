using System.Collections.Generic;
using System.Linq;
using CityBuilder.Domain.Models;

namespace CityBuilder.Domain.Models
{
    /// <summary>
    /// Модель, описывающая статические данные (конфигурацию) типа здания.
    /// </summary>
    public class BuildingTypeConfig
    {
        public BuildingType Type { get; }
        public string Name { get; }
        public int MaxLevel { get; }

        private readonly List<BuildingLevelConfig> _levelData;

        public BuildingTypeConfig(BuildingType type, string name,List<BuildingLevelConfig> levelData)
        {
            this.Type = type;
            this.Name = name;
            this._levelData = levelData;
            this.MaxLevel = levelData.Count;
        }

        /// <summary>
        /// Получает данные для конкретного уровня.
        /// </summary>
        private BuildingLevelConfig GetLevelData(int level)
        {
            if (level - 1 < 0 || level - 1 >= this._levelData.Count)
            {
                return null;
            }
            BuildingLevelConfig data = this._levelData[level - 1];
            return data;
        }

        public IReadOnlyDictionary<ResourceType, int> GetBuildCost()
        {
            return this.GetLevelData(1)?.BuildCost;
        }

        public IReadOnlyDictionary<ResourceType, int> GetUpgradeCost(int currentLevel)
        {
            // Стоимость улучшения хранится в данных *следующего* уровня
            //
            return this.GetLevelData(currentLevel + 1)?.BuildCost;
        }

        public IReadOnlyDictionary<ResourceType, int> GetProduction(int level)
        {
            return this.GetLevelData(level)?.Production;
        }

        public IReadOnlyDictionary<ResourceType, int> GetConsumption(int level)
        {
            return this.GetLevelData(level)?.Consumption;
        }

        public IReadOnlyDictionary<ResourceType, int> GetUnitProductionCost(int level)
        {
            return this.GetLevelData(level)?.UnitProductionCost;
        }
    }
}