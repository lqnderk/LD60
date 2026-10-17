using System;
using Game.UI;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.UI;

namespace Game.Core
{
    public enum ViewpointType
    {
        Monitor,
        Motherboard
    }

    public class ViewpointController : MonoBehaviour
    {
        [Header("Camera & Viewpoints")]
        [SerializeField] private Camera targetCamera;
        [SerializeField] private Transform monitorViewpoint;
        [SerializeField] private Transform motherboardViewpoint;
        [SerializeField] private ViewpointType initialViewpoint = ViewpointType.Monitor;
        [SerializeField] private bool preserveCameraZ = true;
        [SerializeField] private bool syncCameraRotation = false;

        [Header("Arrow Configuration")]
        [SerializeField] private Transform arrowTransform;
        [SerializeField] private GameObject monitorArrow;
        [SerializeField] private GameObject motherboardArrow;
        [SerializeField] private Transform monitorArrowAnchor;
        [SerializeField] private Transform motherboardArrowAnchor;
        [SerializeField] private bool flipArrowOnSwitch = true;
        [SerializeField] private Vector3 arrowRotationMonitor = Vector3.zero;
        [SerializeField] private Vector3 arrowRotationMotherboard = new Vector3(0f, 0f, 180f);

        [Header("Fade Settings")]
        [SerializeField] private float fadeOutDuration = 0.35f;
        [SerializeField] private float holdDuration = 0.1f;
        [SerializeField] private float fadeInDuration = 0.35f;
        [SerializeField] private bool ignoreIfOverUI = true;

        [Header("Events")]
        [SerializeField] private UnityEvent<ViewpointType> onViewpointChanged;

        private ViewpointType currentViewpoint;
        private bool isTransitioning;
        private bool hasInitialized;
        private Vector3 arrowLocalOffsetFromViewpoint;

        public ViewpointType CurrentViewpoint => currentViewpoint;
        public bool IsTransitioning => isTransitioning;
        public event Action<ViewpointType> OnViewpointChanged;

        private void Awake()
        {
            if (targetCamera == null)
            {
                targetCamera = Camera.main;
            }

            currentViewpoint = initialViewpoint;
            CacheInitialArrowOffset();
            RegisterArrowButtons();
        }

        private void Start()
        {
            ApplyViewpointInstant(currentViewpoint);
            hasInitialized = true;
        }

        private void Update()
        {
            if (isTransitioning)
            {
                return;
            }

            if (!WasPointerPressedThisFrame())
            {
                return;
            }

            if (ignoreIfOverUI && EventSystem.current != null && EventSystem.current.IsPointerOverGameObject())
            {
                return;
            }

            Vector2 screenPos = GetPointerScreenPosition();
            if (IsPointerOverArrow(screenPos))
            {
                ToggleViewpoint();
            }
        }

        public void ToggleViewpoint()
        {
            if (isTransitioning)
            {
                return;
            }

            var nextViewpoint = currentViewpoint == ViewpointType.Monitor
                ? ViewpointType.Motherboard
                : ViewpointType.Monitor;

            SwitchToViewpoint(nextViewpoint);
        }

        public void SwitchToViewpoint(ViewpointType targetViewpointType)
        {
            if (isTransitioning)
            {
                return;
            }

            if (hasInitialized && currentViewpoint == targetViewpointType)
            {
                return;
            }

            isTransitioning = true;

            ScreenFader.Instance.FadeOutAndIn(
                fadeOutDuration,
                holdDuration,
                fadeInDuration,
                onMiddle: () =>
                {
                    ApplyViewpointInstant(targetViewpointType);
                },
                onComplete: () =>
                {
                    isTransitioning = false;
                }
            );
        }

        public void GoToMonitor()
        {
            SwitchToViewpoint(ViewpointType.Monitor);
        }

        public void GoToMotherboard()
        {
            SwitchToViewpoint(ViewpointType.Motherboard);
        }

        private void ApplyViewpointInstant(ViewpointType viewpointType)
        {
            currentViewpoint = viewpointType;
            Transform targetVP = viewpointType == ViewpointType.Monitor ? monitorViewpoint : motherboardViewpoint;

            if (targetVP != null && targetCamera != null)
            {
                Vector3 newPosition = targetVP.position;
                if (preserveCameraZ)
                {
                    newPosition.z = targetCamera.transform.position.z;
                }

                targetCamera.transform.position = newPosition;

                if (syncCameraRotation)
                {
                    targetCamera.transform.rotation = targetVP.rotation;
                }
            }

            UpdateArrowDisplay(viewpointType);
            onViewpointChanged?.Invoke(viewpointType);
            OnViewpointChanged?.Invoke(viewpointType);
        }

        private void UpdateArrowDisplay(ViewpointType viewpointType)
        {
            if (monitorArrow != null && motherboardArrow != null)
            {
                monitorArrow.SetActive(viewpointType == ViewpointType.Monitor);
                motherboardArrow.SetActive(viewpointType == ViewpointType.Motherboard);
                return;
            }

            if (arrowTransform == null)
            {
                return;
            }

            arrowTransform.gameObject.SetActive(true);

            if (monitorArrowAnchor != null && motherboardArrowAnchor != null)
            {
                Transform anchor = viewpointType == ViewpointType.Monitor ? monitorArrowAnchor : motherboardArrowAnchor;
                arrowTransform.position = anchor.position;
                arrowTransform.rotation = anchor.rotation;
                return;
            }

            if (targetCamera != null && arrowTransform.parent != targetCamera.transform)
            {
                Transform currentVP = viewpointType == ViewpointType.Monitor ? monitorViewpoint : motherboardViewpoint;
                if (currentVP != null)
                {
                    arrowTransform.position = currentVP.position + arrowLocalOffsetFromViewpoint;
                }
            }

            if (flipArrowOnSwitch)
            {
                arrowTransform.localEulerAngles = viewpointType == ViewpointType.Monitor
                    ? arrowRotationMonitor
                    : arrowRotationMotherboard;
            }
        }

        private void CacheInitialArrowOffset()
        {
            if (arrowTransform != null && monitorViewpoint != null)
            {
                if (targetCamera == null || arrowTransform.parent != targetCamera.transform)
                {
                    arrowLocalOffsetFromViewpoint = arrowTransform.position - monitorViewpoint.position;
                }
            }
        }

        private void RegisterArrowButtons()
        {
            BindButton(arrowTransform);
            if (monitorArrow != null) BindButton(monitorArrow.transform);
            if (motherboardArrow != null) BindButton(motherboardArrow.transform);
        }

        private void BindButton(Transform target)
        {
            if (target == null)
            {
                return;
            }

            var button = target.GetComponent<Button>();
            if (button != null)
            {
                button.onClick.AddListener(ToggleViewpoint);
            }
        }

        private bool IsPointerOverArrow(Vector2 screenPosition)
        {
            Transform currentArrow = GetActiveArrowTransform();
            if (currentArrow == null || targetCamera == null)
            {
                return false;
            }

            Vector3 worldPoint = targetCamera.ScreenToWorldPoint(screenPosition);
            Collider2D hit2D = Physics2D.OverlapPoint(worldPoint);
            if (hit2D != null && (hit2D.transform == currentArrow || hit2D.transform.IsChildOf(currentArrow)))
            {
                return true;
            }

            Ray ray = targetCamera.ScreenPointToRay(screenPosition);
            if (Physics.Raycast(ray, out RaycastHit hit3D))
            {
                if (hit3D.transform == currentArrow || hit3D.transform.IsChildOf(currentArrow))
                {
                    return true;
                }
            }

            return false;
        }

        private Transform GetActiveArrowTransform()
        {
            if (monitorArrow != null && motherboardArrow != null)
            {
                return currentViewpoint == ViewpointType.Monitor ? monitorArrow.transform : motherboardArrow.transform;
            }

            return arrowTransform;
        }

        private bool WasPointerPressedThisFrame()
        {
            if (Pointer.current != null && Pointer.current.press.wasPressedThisFrame)
            {
                return true;
            }

            if (Mouse.current != null && Mouse.current.leftButton.wasPressedThisFrame)
            {
                return true;
            }

            if (Touchscreen.current != null && Touchscreen.current.primaryTouch.press.wasPressedThisFrame)
            {
                return true;
            }

            return false;
        }

        private Vector2 GetPointerScreenPosition()
        {
            if (Pointer.current != null)
            {
                return Pointer.current.position.ReadValue();
            }

            if (Mouse.current != null)
            {
                return Mouse.current.position.ReadValue();
            }

            if (Touchscreen.current != null)
            {
                return Touchscreen.current.primaryTouch.position.ReadValue();
            }

            return Vector2.zero;
        }
    }
}
