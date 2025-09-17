using UnityEngine;

namespace CityBuilder.Domain.MessageDTO
{
    public struct CameraControlDTO
    {
        public Vector2 MoveInput;
        public Vector2 ZoomInput;
        public Vector2 PointerDelta; // Движение мыши с прошлого кадра
        public bool IsDragActive;
    }
}