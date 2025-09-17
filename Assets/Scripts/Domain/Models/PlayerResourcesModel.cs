using System.Collections.Generic;

namespace CityBuilder.Domain.Models
{
    /// <summary>
    /// Модель, хранящая состояние всех ресурсов игрока.
    /// </summary>
    public class PlayerResourcesModel
    {        
        private readonly Dictionary<ResourceType, int> _resources = new();

        public PlayerResourcesModel()
        {
            this.Add(ResourceType.Gold, 500);
        }

        /// <summary>
        /// Получает текущее количество указанного ресурса.
        /// </summary>
        public int GetAmount(ResourceType type)
        {
            this._resources.TryGetValue(type, out int amount);
            return amount;
        }

        /// <summary>
        /// Добавляет указанное количество ресурса.
        /// </summary>
        public void Add(ResourceType type, int amount)
        {
            if (amount <= 0) return;
            this._resources[type] = this.GetAmount(type) + amount;
        }

        /// <summary>
        /// Проверяет, достаточно ли у игрока ресурсов для покрытия стоимости.
        /// </summary>
        /// <param name="cost">Словарь "Ресурс -> Требуемое количество"</param>
        public bool HasEnough(IReadOnlyDictionary<ResourceType, int> cost)
        {
            if (cost == null) return true;
            
            foreach (KeyValuePair<ResourceType, int> item in cost)
            {
                if (this.GetAmount(item.Key) < item.Value)
                {
                    return false;
                }
            }
            return true;
        }

        /// <summary>
        /// Пытается потратить ресурсы.
        /// </summary>
        /// <param name="cost">Ресурс -> Требуемое количество</param>
        /// <returns>True, если ресурсы были успешно потрачены, иначе false.</returns>
        public bool Spend(IReadOnlyDictionary<ResourceType, int> cost)
        {
            if (cost == null) return true;
            
            if (!this.HasEnough(cost))
            {
                return false;
            }
            
            foreach (KeyValuePair<ResourceType, int> item in cost)
            {
                this._resources[item.Key] -= item.Value;
            }
            return true;
        }
        
        /// <summary>
        /// (Для тестов) Прямая установка значения ресурса.
        /// </summary>
        public void SetAmount(ResourceType type, int amount)
        {
            this._resources[type] = amount;
        }

        public void Clear()
        {
            this._resources.Clear();
        }
    }
}