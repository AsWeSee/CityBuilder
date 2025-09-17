using CityBuilder.Domain.Models;
using UnityEngine;

namespace CityBuilder.Domain.MessageDTO
{
    public struct BuildingPlacedEventDTO
    {
        public int InstanceId;
        public BuildingType BuildingType;
        public Vector2Int Position;
    }
}