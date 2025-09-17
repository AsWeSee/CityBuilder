using UnityEngine;
using CityBuilder.Domain.Models;
using System.Collections.Generic;

namespace CityBuilder.Repositories
{


    [CreateAssetMenu(fileName = "NewBuildingType", menuName = "City Builder/Building Type")]
    public class BuildingTypeSO : ScriptableObject
    {
        [SerializeField] private BuildingType _type;
        [SerializeField] private string _buildingName;

        [Tooltip("Настройки для каждого уровня. Element 0 = Уровень 1, Element 1 = Уровень 2 и т.д.")]
        [SerializeField] private List<BuildingLevelConfigSO> _levels;

        public BuildingType Type => this._type;

        public BuildingTypeConfig ToDomainModel()
        {
            List<BuildingLevelConfig> levelDataList = new List<BuildingLevelConfig>();

            for (int i = 0; i < this._levels.Count; i++)
            {
                BuildingLevelConfigSO config = this._levels[i];

                levelDataList.Add(new BuildingLevelConfig
                {
                    BuildCost = this.ConvertToDict(config.BuildCost),
                    Production = this.ConvertToDict(config.Production),
                    Consumption = this.ConvertToDict(config.Consumption),
                    UnitProductionCost = this.ConvertToDict(config.UnitProductionCost)
                });
            }

            return new BuildingTypeConfig(this._type, this._buildingName, levelDataList);
        }

        /// <summary>
        /// Безопасно конвертирует список в словарь.
        /// Если встречаются дубликаты ресурсов, их значения суммируются.
        /// </summary>
        private IReadOnlyDictionary<ResourceType, int> ConvertToDict(List<ResourceValue> list)
        {
            Dictionary<ResourceType, int> dict = new Dictionary<ResourceType, int>();
            if (list == null || list.Count == 0)
            {
                return dict;
            }

            foreach (ResourceValue item in list)
            {
                if (dict.ContainsKey(item.Type))
                {
                    // Если ресурс уже есть в словаре, суммируем количество
                    dict[item.Type] += item.Amount;
                }
                else
                {
                    // Иначе просто добавляем
                    dict.Add(item.Type, item.Amount);
                }
            }
            return dict;
        }
    }
}