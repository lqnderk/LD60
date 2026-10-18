using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;
using UnityEngine.EventSystems;

namespace Game.UI
{
    public class UIWindow : MonoBehaviour, IPointerClickHandler
    {
        private static readonly List<UIWindow> openWindows = new();

        public static bool HasActiveWindows
        {
            get
            {
                CleanupClosedWindows();
                return openWindows.Count > 0;
            }
        }

        [SerializeField] private GameObject window;
        [SerializeField] private GameObject objectToHide;
        [SerializeField] private Button closeButton;
        [SerializeField] private GameObject closeTrigger;
        [SerializeField] private bool openOnClick = true;
        [SerializeField] private bool closeOnEscape = true;
        [SerializeField] private bool startClosed = true;
        [SerializeField] private UnityEvent onOpen;
        [SerializeField] private UnityEvent onClose;

        public GameObject Window
        {
            get => window;
            set => window = value;
        }

        public GameObject ObjectToHide
        {
            get => objectToHide;
            set => objectToHide = value;
        }

        public Button CloseButton
        {
            get => closeButton;
            set => closeButton = value;
        }

        public GameObject CloseTrigger
        {
            get => closeTrigger;
            set => closeTrigger = value;
        }

        public bool CloseOnEscape
        {
            get => closeOnEscape;
            set => closeOnEscape = value;
        }

        public bool IsOpen => window != null && window.activeSelf;

        private void Awake()
        {
            var button = GetComponent<Button>();
            if (button != null && openOnClick)
            {
                button.onClick.AddListener(Open);
            }

            if (closeButton != null)
            {
                closeButton.onClick.AddListener(Close);
            }

            if (closeTrigger != null)
            {
                var trigger = closeTrigger.GetComponent<UIWindowTrigger>();
                if (trigger == null)
                {
                    trigger = closeTrigger.AddComponent<UIWindowTrigger>();
                }
                trigger.Initialize(this, UIWindowTrigger.TriggerAction.Close);
            }
        }

        private void Start()
        {
            if (startClosed && window != null)
            {
                window.SetActive(false);
            }
        }

        private void OnMouseDown()
        {
            if (openOnClick)
            {
                Open();
            }
        }

        public void OnPointerClick(PointerEventData eventData)
        {
            if (openOnClick)
            {
                Open();
            }
        }

        public void Open()
        {
            if (window != null)
            {
                if (window.activeSelf && openWindows.Contains(this))
                {
                    return;
                }

                window.SetActive(true);
            }

            if (objectToHide != null)
            {
                objectToHide.SetActive(false);
            }

            if (!openWindows.Contains(this))
            {
                openWindows.Add(this);
            }

            onOpen?.Invoke();
        }

        public void Close()
        {
            openWindows.Remove(this);

            if (window != null)
            {
                window.SetActive(false);
            }

            if (objectToHide != null)
            {
                objectToHide.SetActive(true);
            }

            onClose?.Invoke();
        }

        public void Toggle()
        {
            if (IsOpen)
            {
                Close();
            }
            else
            {
                Open();
            }
        }

        private void Update()
        {
            if (!closeOnEscape || !Input.GetKeyDown(KeyCode.Escape))
            {
                return;
            }

            CleanupClosedWindows();

            if (openWindows.Count > 0 && openWindows[openWindows.Count - 1] == this)
            {
                Close();
            }
        }

        private void OnDisable()
        {
            openWindows.Remove(this);
        }

        private void OnDestroy()
        {
            openWindows.Remove(this);
        }

        private static void CleanupClosedWindows()
        {
            for (int i = openWindows.Count - 1; i >= 0; i--)
            {
                if (openWindows[i] == null || (openWindows[i].window != null && !openWindows[i].window.activeSelf))
                {
                    openWindows.RemoveAt(i);
                }
            }
        }
    }
}
