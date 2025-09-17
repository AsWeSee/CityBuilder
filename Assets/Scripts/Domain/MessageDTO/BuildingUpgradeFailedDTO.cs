namespace CityBuilder.Domain.MessageDTO
{
    public enum UpgradeFailureReason { NotSelected, MaxLevelReached, NotEnoughResources }

    public struct BuildingUpgradeFailedDTO
    {
        public UpgradeFailureReason Reason;
    }
}