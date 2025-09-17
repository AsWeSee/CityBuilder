using System.Collections.Generic;

namespace CityBuilder.Domain.Models
{
    /// <summary>
    /// Хранит данные для одного уровня здания (стоимость, производство и т.д.)
    /// </summary>
    public class BuildingLevelConfig
    {
        public int Level;
        
        /// <summary>
        /// Стоимость постройки если 1 уровень и апгрейда если 2 и выше 
        /// </summary>
        public IReadOnlyDictionary<ResourceType, int> BuildCost;
        
        /// <summary>
        /// Ресурсы, производимые зданием за "тик".
        /// </summary>
        public IReadOnlyDictionary<ResourceType, int> Production;
        
        /// <summary>
        /// Ресурсы, потребляемые зданием за "тик".
        /// </summary>
        public IReadOnlyDictionary<ResourceType, int> Consumption;
        
        /// <summary>
        /// Стоимость производства юнита в этом здании (для Barracks, Stable).
        /// </summary>
        public IReadOnlyDictionary<ResourceType, int> UnitProductionCost;
    }

    
}