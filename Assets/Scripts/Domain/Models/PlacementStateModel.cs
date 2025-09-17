namespace CityBuilder.Domain.Models
{
    /// <summary>
    /// Хранит текущее состояние режима постройки.
    /// </summary>
    public class PlacementStateModel
    {
        /// <summary>
        /// ID типа здания, выбранного для постройки. Null, если не в режиме постройки.
        /// </summary>
        public BuildingType? SelectedBuildingType { get; set; }

        /// <summary>
        /// Текущий угол поворота для строящегося здания.
        /// </summary>
        public float PlacementRotationY { get; set; }
        public bool IsInPlacementMode => this.SelectedBuildingType.HasValue;
        
        public void Reset()
        {
            this.SelectedBuildingType = null;
            this.PlacementRotationY = 0f;
        }
    }
}