using UnityEngine;
using VContainer;
using MessagePipe;
using System;
using CityBuilder.Domain.MessageDTO;
using VContainer.Unity;

namespace CityBuilder.Presentation.Gameplay
{
    public class CameraOperator : MonoBehaviour, IInitializable, IDisposable
    {

        [Header("Настройки (TODO: вынести в настройки)")]
        [Tooltip("Размер сетки для автоматической установки камеры")]
        [SerializeField] private Vector2Int _gridSize = new(32, 32);
        [SerializeField] private float _moveSpeed = 15f;
        [SerializeField] private float _zoomSpeed = 5f;
        [SerializeField] private Vector2 _zoomLimits = new(5f, 20f); // Теперь это orthographicSize
        [SerializeField] private float _dragSpeed = 1f;
        
        [Header("Начальная Позиция и Угол")]
        [SerializeField] private float _initialHeight = 20f;
        [SerializeField] private Vector3 _initialRotation = new(45f, -45f, 0f);
        
        private ISubscriber<CameraControlDTO> _cameraControlSubscriber;
        private IDisposable _disposable;
        private Camera _camera;
        

        [Inject]
        public void Construct(ISubscriber<CameraControlDTO> cameraControlSubscriber)
        {
            this._cameraControlSubscriber = cameraControlSubscriber;
        }
        private void Awake()
        {
            this._camera = this.GetComponent<Camera>();
            if (!this._camera.orthographic)
            {
                Debug.LogWarning("CameraOperator предназначен для ортографической камеры!", this);
            }
        }


        public void Initialize()
        {
            this.SetupInitialPositionAndRotation();
            this._disposable = this._cameraControlSubscriber.Subscribe(this.OnCameraControl);
        }

        private void SetupInitialPositionAndRotation()
        {
            if (this._gridSize.x <= 0 || this._gridSize.y <= 0) return;

            this.transform.position = new Vector3(this._gridSize.x, this._initialHeight, 0);
            
            Vector3 gridCenter = new Vector3(this._gridSize.x / 2f, 0, this._gridSize.y / 2f);
            this.transform.LookAt(gridCenter);
            // transform.rotation = Quaternion.Euler(_initialRotation);
        }

        private void OnCameraControl(CameraControlDTO dto)
        {
            // Перемещение с клавиатуры
            this.HandleMovement(dto.MoveInput);

            // Зум колесом мыши
            this._camera.orthographicSize -= dto.ZoomInput.y * this._zoomSpeed * Time.deltaTime;
            this._camera.orthographicSize = Mathf.Clamp(this._camera.orthographicSize, this._zoomLimits.x, this._zoomLimits.y);

            // Перемещение с зажатой ПКМ (Drag)
            // TODO: соотношение зума к лимиту и скорость перемещения стоит вынести в настройки
            if (dto.IsDragActive)
            {
               
                this.HandleDragMovement(dto.PointerDelta);
                }
        }

        private void HandleMovement(Vector2 moveInput)
        {
            // Проецируем вектор "вперед" камеры на горизонтальную плоскость (XZ)
            Vector3 forward = Vector3.ProjectOnPlane(this.transform.forward, Vector3.up).normalized;
            Vector3 right = this.transform.right.normalized; // Вектор "вправо" уже на горизонтальной плоскости

            // Складываем векторы движения
            Vector3 moveDirection = (forward * moveInput.y + right * moveInput.x).normalized;
            this.transform.position += moveDirection * this._moveSpeed * Time.deltaTime;
        }

        private void HandleDragMovement(Vector2 pointerDelta)
        {
            // Коэффициент скорости перетаскивания зависит от текущего зума,
            // чтобы движение ощущалось одинаково на разном отдалении.
            float dragMultiplier = (this._camera.orthographicSize / this._zoomLimits.x) * this._dragSpeed;

            // Конвертируем дельту из пикселей экрана в мировые единицы
            Vector3 deltaInWorld = new Vector3(pointerDelta.x, pointerDelta.y, 0) * 0.01f * dragMultiplier;

            // Применяем смещение в локальных координатах камеры, но обнуляем Z,
            // чтобы движение было параллельно экрану
            this.transform.Translate(-deltaInWorld.x, -deltaInWorld.y, 0, Space.Self);
        }
        
        
        public void Dispose() => this. _disposable?.Dispose();
    }
}