using UnityEngine;

namespace CityBuilder.Domain.MessageDTO
{
    /// <summary>
    /// Сообщает, что курсор переместился на новую клетку сетки.
    /// </summary>
    public struct PointerGridPositionChangedDTO
    {
        public Vector2Int GridPosition;
        public bool IsInBounds;
    }
}