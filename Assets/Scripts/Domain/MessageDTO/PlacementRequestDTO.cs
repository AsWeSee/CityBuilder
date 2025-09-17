using UnityEngine;

namespace CityBuilder.Domain.MessageDTO
{
    /// <summary>
    /// Сообщает, что игрок кликнул для размещения здания на указанной клетке.
    /// </summary>
    public struct PlacementRequestedDTO
    {
        public Vector2Int GridPosition;
    }
}