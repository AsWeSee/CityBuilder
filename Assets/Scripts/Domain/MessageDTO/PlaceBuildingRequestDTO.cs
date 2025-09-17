using CityBuilder.Domain.Models;
using UnityEngine;

namespace CityBuilder.Domain.MessageDTO
{
    public struct PlaceBuildingRequestDTO
    {
        public BuildingType BuildingType;
        public Vector2Int Position;
    }
}