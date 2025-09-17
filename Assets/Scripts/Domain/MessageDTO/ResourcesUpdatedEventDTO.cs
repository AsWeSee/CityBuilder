using CityBuilder.Domain.Models;

namespace CityBuilder.Infrastructure.MessagesDTO
{
    /// <summary>
    /// Сообщение от UseCase/Service к Presenter: "Количество ресурса изменилось".
    /// </summary>
    public struct ResourcesUpdatedEventDTO
    {
        public ResourceType ResourceType;
        public int NewAmount;
    }
}