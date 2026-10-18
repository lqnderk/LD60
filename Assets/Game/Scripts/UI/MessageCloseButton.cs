using UnityEngine;
using UnityEngine.UI;

namespace Game.UI
{
    public class MessageCloseButton : MonoBehaviour, IWorldClickTarget
    {
        [SerializeField] private GameObject message;

        private MessageSpawner spawner;

        public void Initialize(GameObject target, MessageSpawner owner)
        {
            message = target;
            spawner = owner;
        }

        private void Awake()
        {
            var button = GetComponent<Button>();

            if (button != null)
            {
                button.onClick.AddListener(Close);
            }
        }

        private void OnMouseDown()
        {
            Close();
        }

        public void OnWorldClick()
        {
            Close();
        }

        public void Close()
        {
            if (message == null)
            {
                return;
            }

            GameObject target = message;
            message = null;

            if (spawner != null)
            {
                spawner.Remove(target);
                return;
            }

            Destroy(target);
        }
    }
}
