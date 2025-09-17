using R3;
using UnityEngine;

namespace CityBuilder.Domain.Models
{
    /// <summary>
    /// Хранит состояние того, какой объект в данный момент выбран игроком.
    /// </summary>
    public class SelectionModel
    {
        /// <summary>
        /// Позиция экземпляра выбранного здания. Null, если ничего не выбрано.
        /// </summary>
        public ReactiveProperty<Vector2Int?> SelectedBuildingPosition { get; } = new();
    }
}