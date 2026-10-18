using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

namespace Game.UI
{
    public class UIWindowTrigger : MonoBehaviour, IPointerClickHandler, IWorldClickTarget
    {
        public enum TriggerAction
        {
            Close,
            Open,
            Toggle
        }

        [SerializeField] private UIWindow targetWindow;
        [SerializeField] private TriggerAction action = TriggerAction.Close;

        public UIWindow TargetWindow
        {
            get => targetWindow;
            set => targetWindow = value;
        }

        public TriggerAction Action
        {
            get => action;
            set => action = value;
        }

        public void Initialize(UIWindow window, TriggerAction triggerAction = TriggerAction.Close)
        {
            targetWindow = window;
            action = triggerAction;
        }

        private void Awake()
        {
            var button = GetComponent<Button>();
            if (button != null)
            {
                button.onClick.AddListener(ExecuteAction);
            }
        }

        private void OnMouseDown()
        {
            if (WorldClickRouter.LastHandledFrame == Time.frameCount)
            {
                return;
            }

            ExecuteAction();
        }

        public void OnWorldClick()
        {
            ExecuteAction();
        }

        public void OnPointerClick(PointerEventData eventData)
        {
            ExecuteAction();
        }

        public void ExecuteAction()
        {
            if (targetWindow == null)
            {
                return;
            }

            switch (action)
            {
                case TriggerAction.Open:
                    targetWindow.Open();
                    break;
                case TriggerAction.Close:
                    targetWindow.Close();
                    break;
                case TriggerAction.Toggle:
                    targetWindow.Toggle();
                    break;
            }
        }
    }
}
