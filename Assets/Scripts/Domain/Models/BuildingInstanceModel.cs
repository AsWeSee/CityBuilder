using UnityEngine; // Vector2Int - это структура Unity, но она считается "чистой" 
                   // т.к. не имеет логики и зависимостей от движка.

namespace CityBuilder.Domain.Models
{
    /// <summary>
    /// Модель, представляющая экземпляр здания на игровой сетке.
    /// </summary>
    public class BuildingInstanceModel
    {
        /// <summary>
        /// Уникальный ID этого экземпляра здания.
        /// </summary>
        public int InstanceId { get; }
        
        /// <summary>
        /// ID типа здания (ссылка на BuildingType).
        /// </summary>
        public BuildingType Type { get; }
        
        public Vector2Int Position { get; private set; }
        public int Level { get; private set; }

        public BuildingInstanceModel(int instanceId, BuildingType type, Vector2Int position, int level = 1)
        {
            this.InstanceId = instanceId;
            this.Type = type;
            this.Position = position;
            this.Level = level;
        }

        /// <summary>
        /// Проверяет, можно ли улучшить это здание, исходя из его конфига.
        /// </summary>
        public bool CanUpgrade(BuildingTypeConfig typeConfig)
        {
            if (typeConfig.Type != this.Type)
                return false; // Неверный конфиг
            
            return this.Level < typeConfig.MaxLevel;
        }

        /// <summary>
        /// Повышает уровень здания. (Проверка стоимости - в UseCase).
        /// </summary>
        public void LevelUp()
        {
            this.Level++;
        }

        /// <summary>
        /// Изменяет позицию здания. (Проверка - в GridModel).
        /// </summary>
        public void Move(Vector2Int newPosition)
        {
            this.Position = newPosition;
        }
    }
}