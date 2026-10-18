using UnityEngine;

namespace Game.UI
{
    public class HudHider : MonoBehaviour
    {
        [SerializeField] private GameObject screen;
        [SerializeField] private GameObject[] hudObjects;

        private bool hidden;

        private void LateUpdate()
        {
            Apply(screen != null && screen.activeInHierarchy);
        }

        private void Apply(bool hide)
        {
            if (hide == hidden)
            {
                return;
            }

            hidden = hide;

            if (hudObjects == null)
            {
                return;
            }

            for (int i = 0; i < hudObjects.Length; i++)
            {
                if (hudObjects[i] != null)
                {
                    hudObjects[i].SetActive(!hide);
                }
            }
        }
    }
}
