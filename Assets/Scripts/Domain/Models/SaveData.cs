using System.Collections.Generic;
using UnityEngine; // Для Vector2Int

namespace CityBuilder.Domain.Models
{
    // Отдельная структура для хранения данных одного здания
    [System.Serializable]
    public class BuildingSaveData
    {
        public int InstanceId;
        public int TypeId;
        public int Level;
        public Vector2Int Position;
    }

    // Отдельная структура для хранения данных одного ресурса
    [System.Serializable]
    public class ResourceSaveData
    {
        public ResourceType ResourceType;
        public int Amount;
    }

    /// <summary>
    /// Основной класс, который будет сериализован в JSON.
    /// </summary>
    [System.Serializable]
    public class SaveData
    {
        public List<BuildingSaveData> Buildings = new();
        public List<ResourceSaveData> Resources = new();
    }
}