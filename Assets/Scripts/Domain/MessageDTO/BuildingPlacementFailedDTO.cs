namespace CityBuilder.Domain.MessageDTO
{
    public enum FailureReason { CellIsOccupied, NotEnoughResources }
    
    public struct BuildingPlacementFailedDTO
    {
        public FailureReason Reason;
    }
}