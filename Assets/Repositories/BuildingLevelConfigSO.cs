using UnityEngine;
using System.Collections.Generic;
using CityBuilder.Domain.Models;

namespace CityBuilder.Repositories
{
    [System.Serializable]
    public class BuildingLevelConfigSO
    {
        [Header("Стоимость Постройки / Улучшения")]
        public List<ResourceValue> BuildCost; 
        
        [Header("Производство и Потребление за цикл")]
        [Space(5)]
        public List<ResourceValue> Production;
        public List<ResourceValue> Consumption;
        
        [Header("Стоимость найма юнитов")]
        [Space(5)]
        public List<ResourceValue> UnitProductionCost;
    }
}