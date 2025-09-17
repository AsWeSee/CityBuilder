using UnityEngine;
using VContainer;
using MessagePipe;
using UnityEngine.InputSystem;
using CityBuilder.Domain.MessageDTO;
using CityBuilder.Domain.Models;
using System;

namespace CityBuilder.Infrastructure
{
    public class InputAdapter : MonoBehaviour
    {
        private IPublisher<PointerGridPositionChangedDTO> _pointerPosPublisher;
        private IPublisher<PlacementRequestedDTO> _placementReqPublisher;
        private IPublisher<PlacementCanceledDTO> _placementCancelPublisher;
        private IPublisher<CameraControlDTO> _cameraControlPublisher;
        private IPublisher<RotateBuildingRequestDTO> _rotateBuildingPublisher;
        private IPublisher<DeleteSelectedBuildingRequestDTO> _deleteSelectedBuildingPublisher;
        private IPublisher<SelectBuildingToPlaceRequestDTO> _selectBuildingToPlacePublisher;
        private PlayerControls _playerControls;
        private Camera _mainCamera;


        [SerializeField] private LayerMask _groundLayerMask;
        [SerializeField] private float _cellSize = 1f;

        private Vector2Int _lastGridPosition = new Vector2Int(-1, -1);
        private CameraControlDTO _cameraControlDTO = new CameraControlDTO();

        [Inject]
        public void Construct(
            IPublisher<PointerGridPositionChangedDTO> pointerPosPublisher,
            IPublisher<PlacementRequestedDTO> placementReqPublisher,
            IPublisher<PlacementCanceledDTO> placementCancelPublisher,
            IPublisher<CameraControlDTO> cameraControlPublisher,
            IPublisher<RotateBuildingRequestDTO> rotateBuildingPublisher,
            IPublisher<DeleteSelectedBuildingRequestDTO> deleteSelectedBuildingPublisher,
            IPublisher<SelectBuildingToPlaceRequestDTO> selectBuildingToPlacePublisher)
        {
            this._pointerPosPublisher = pointerPosPublisher;
            this._placementReqPublisher = placementReqPublisher;
            this._placementCancelPublisher = placementCancelPublisher;
            this._cameraControlPublisher = cameraControlPublisher;
            this._rotateBuildingPublisher = rotateBuildingPublisher;
            this._deleteSelectedBuildingPublisher = deleteSelectedBuildingPublisher;
            this._selectBuildingToPlacePublisher = selectBuildingToPlacePublisher;
        }

        private void Awake()
        {
            this._playerControls = new PlayerControls();
            this._mainCamera = Camera.main;
        }

        private void OnEnable()
        {
            this._playerControls.Gameplay.Enable();
            this._playerControls.Gameplay.PointerPosition.performed += this.OnPointerMove;
            this._playerControls.Gameplay.Place.performed += this.OnPlace;
            this._playerControls.Gameplay.Cancel.performed += this.OnCancel;

            this._playerControls.Gameplay.SelectBuilding1.performed += ctx => this.OnSelectBuildingHotkey(0);
            this._playerControls.Gameplay.SelectBuilding2.performed += ctx => this.OnSelectBuildingHotkey(1);
            this._playerControls.Gameplay.SelectBuilding3.performed += ctx => this.OnSelectBuildingHotkey(2);
            this._playerControls.Gameplay.SelectBuilding4.performed += ctx => this.OnSelectBuildingHotkey(3);
            this._playerControls.Gameplay.SelectBuilding5.performed += ctx => this.OnSelectBuildingHotkey(4);
            this._playerControls.Gameplay.SelectBuilding6.performed += ctx => this.OnSelectBuildingHotkey(5);
            this._playerControls.Gameplay.SelectBuilding7.performed += ctx => this.OnSelectBuildingHotkey(6);
            this._playerControls.Gameplay.SelectBuilding8.performed += ctx => this.OnSelectBuildingHotkey(7);
            this._playerControls.Gameplay.SelectBuilding9.performed += ctx => this.OnSelectBuildingHotkey(8);

            this._playerControls.Gameplay.RotateBuilding.performed += this.OnRotateBuilding;
            this._playerControls.Gameplay.DeleteBuilding.performed += this.OnDeleteBuilding;
        }


        private void OnDisable()
        {
            this._playerControls.Gameplay.Disable();
            this._playerControls.Gameplay.PointerPosition.performed -= this.OnPointerMove;
            this._playerControls.Gameplay.Place.performed -= this.OnPlace;
            this._playerControls.Gameplay.Cancel.performed -= this.OnCancel;

            this._playerControls.Gameplay.SelectBuilding1.performed -= ctx => this.OnSelectBuildingHotkey(0);
            this._playerControls.Gameplay.SelectBuilding2.performed -= ctx => this.OnSelectBuildingHotkey(1);
            this._playerControls.Gameplay.SelectBuilding3.performed -= ctx => this.OnSelectBuildingHotkey(2);
            this._playerControls.Gameplay.SelectBuilding4.performed -= ctx => this.OnSelectBuildingHotkey(3);
            this._playerControls.Gameplay.SelectBuilding5.performed -= ctx => this.OnSelectBuildingHotkey(4);
            this._playerControls.Gameplay.SelectBuilding6.performed -= ctx => this.OnSelectBuildingHotkey(5);
            this._playerControls.Gameplay.SelectBuilding7.performed -= ctx => this.OnSelectBuildingHotkey(6);
            this._playerControls.Gameplay.SelectBuilding8.performed -= ctx => this.OnSelectBuildingHotkey(7);
            this._playerControls.Gameplay.SelectBuilding9.performed -= ctx => this.OnSelectBuildingHotkey(8);

            this._playerControls.Gameplay.RotateBuilding.performed -= this.OnRotateBuilding;
            this._playerControls.Gameplay.DeleteBuilding.performed -= this.OnDeleteBuilding;
        }


        private void Update()
        {
            // CameraControlDTO переиспользуется чтобы не создавать в апдейте новый объект
            this._cameraControlDTO.MoveInput = this._playerControls.Gameplay.CameraMove.ReadValue<Vector2>();
            this._cameraControlDTO.ZoomInput = this._playerControls.Gameplay.CameraZoom.ReadValue<Vector2>();
            this._cameraControlDTO.PointerDelta = this._playerControls.Gameplay.PointerDelta.ReadValue<Vector2>();
            this._cameraControlDTO.IsDragActive = this._playerControls.Gameplay.CameraDrag.IsPressed();

            this._cameraControlPublisher.Publish(this._cameraControlDTO);
        }

        private void OnPointerMove(InputAction.CallbackContext context)
        {
            Vector2 screenPosition = context.ReadValue<Vector2>();
            Vector2Int gridPosition = this.GetGridPositionFromScreen(screenPosition);


            // TODO переместить это в презентер
            if (gridPosition != this._lastGridPosition)
            {
                this._lastGridPosition = gridPosition;
                this._pointerPosPublisher.Publish(new PointerGridPositionChangedDTO
                {
                    GridPosition = gridPosition,
                    IsInBounds = gridPosition.x >= 0 && gridPosition.y >= 0
                });
            }
        }

        private void OnPlace(InputAction.CallbackContext context)
        {
            Vector2Int gridPosition = this.GetGridPositionFromScreen(this._playerControls.Gameplay.PointerPosition.ReadValue<Vector2>());

            this._placementReqPublisher.Publish(new PlacementRequestedDTO { GridPosition = gridPosition });
        }

        private void OnCancel(InputAction.CallbackContext context)
        {
            this._placementCancelPublisher.Publish(new PlacementCanceledDTO { });
        }

        private Vector2Int GetGridPositionFromScreen(Vector2 screenPosition)
        {
            Ray ray = this._mainCamera.ScreenPointToRay(screenPosition);
            if (Physics.Raycast(ray, out RaycastHit hit, 200f, this._groundLayerMask))
            {
                int x = Mathf.FloorToInt(hit.point.x / this._cellSize);
                int z = Mathf.FloorToInt(hit.point.z / this._cellSize);
                return new Vector2Int(x, z);
            }
            return new Vector2Int(-1, -1);
        }


        private void OnSelectBuildingHotkey(int buildingTypeId)
        {
            this._selectBuildingToPlacePublisher.Publish(new SelectBuildingToPlaceRequestDTO { BuildingType = (BuildingType)buildingTypeId });
        }

        private void OnRotateBuilding(InputAction.CallbackContext context)
        {
            this._rotateBuildingPublisher.Publish(new RotateBuildingRequestDTO());
        }

        private void OnDeleteBuilding(InputAction.CallbackContext context)
        {
            this._deleteSelectedBuildingPublisher.Publish(new DeleteSelectedBuildingRequestDTO());
        }

    }
}