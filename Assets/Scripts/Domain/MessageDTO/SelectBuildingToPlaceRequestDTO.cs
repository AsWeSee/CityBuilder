using CityBuilder.Domain.Models;

namespace CityBuilder.Domain.MessageDTO
{
    /// <summary>
    /// Сообщение от Presenter к UseCase: "Игрок хочет начать постройку этого здания".
    /// </summary>
    public struct SelectBuildingToPlaceRequestDTO
    {
        public BuildingType BuildingType;
    }
}