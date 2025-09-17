using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using CityBuilder.Domain.Models;
using CityBuilder.Repositories; // Важно: он знает о Domain-моделях

namespace CityBuilder.Repositories
{
    /// <summary>
    /// Центральный репозиторий для всех конфигураций зданий.
    /// Выступает в роли "моста", предоставляя данные из ScriptableObject'ов
    /// в виде чистых Domain-моделей для остальной части приложения.
    /// </summary>
    [CreateAssetMenu(fileName = "BuildingConfigurationProvider", menuName = "City Builder/Building Configuration Provider")]
    public class BuildingConfigurationProvider : ScriptableObject
    {
        // В этот список в инспекторе Unity вы перетаскиваете все ваши
        // ассеты BuildingTypeSO (House, Farm, Mine и т.д.).
        [SerializeField] 
        private List<BuildingTypeSO> _buildingTypes;

        // Кэш для быстрого доступа к уже сконвертированным, чистым Domain-моделям.
        // Словарь (Dictionary) обеспечивает почти мгновенный поиск по ID.
        private IReadOnlyDictionary<BuildingType, BuildingTypeConfig> _buildingDataCache;

        /// <summary>
        /// Инициализирует провайдер. Конвертирует все SO в Domain-модели и кэширует их.
        /// Этот метод должен быть вызван один раз при старте игры (например, из Installer'а).
        /// </summary>
        public void Initialize()
        {
            // Если кэш уже создан, ничего не делаем.
            if (this._buildingDataCache != null) return;

            Debug.Log($"[BuildingConfigurationProvider] Initializing with {this._buildingTypes.Count} building types...");

            // Используем LINQ (ToDictionary) для элегантной конвертации списка SO в словарь
            // чистых Domain-моделей.
            this._buildingDataCache = this._buildingTypes.ToDictionary(
                // Ключом словаря будет ID типа здания, взятый из SO.
                so => so.Type, 
                // Значением будет чистая Domain-модель, полученная вызовом метода-адаптера.
                so => so.ToDomainModel()
            );
        }

        /// <summary>
        /// Основной метод, используемый системой (например, UseCases) для получения
        /// конфигурации здания по его уникальному идентификатору типа.
        /// </summary>
        /// <param name="typeId">ID типа здания.</param>
        /// <returns>Чистая модель данных BuildingTypeData или null, если ID не найден.</returns>
        public BuildingTypeConfig GetBuildingConfigByType(BuildingType type)
        {
            if (this._buildingDataCache == null)
            {
                Debug.LogError("[BuildingConfigurationProvider] Not initialized! Call Initialize() before use.");
                return null;
            }

            this._buildingDataCache.TryGetValue(type, out BuildingTypeConfig data);
            return data;
        }

        /// <summary>
        /// Возвращает перечисление всех доступных типов зданий.
        /// Может быть полезно для UI, чтобы динамически создать кнопки для всех зданий.
        /// </summary>
        public IEnumerable<BuildingTypeConfig> GetAllBuildingTypes()
        {
            if (this._buildingDataCache == null)
            {
                Debug.LogError("[BuildingConfigurationProvider] Not initialized! Call Initialize() before use.");
                return Enumerable.Empty<BuildingTypeConfig>();
            }
            
            return this._buildingDataCache.Values;
        }
    }
}