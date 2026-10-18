using UnityEngine;

namespace Game.UI
{
    public class HandAnimator : MonoBehaviour
    {
        private static HandAnimator instance;

        public static HandAnimator Instance => instance;

        [SerializeField] private GameObject idleHand;
        [SerializeField] private GameObject clickHand;
        [SerializeField] private float clickDuration = 0.14f;

        private float timer;

        private void Awake()
        {
            if (instance != null && instance != this)
            {
                Destroy(this);
                return;
            }

            instance = this;
            Apply(false);
        }

        private void OnDestroy()
        {
            if (instance == this)
            {
                instance = null;
            }
        }

        public void Punch()
        {
            timer = Mathf.Max(clickDuration, 0.01f);
            Apply(true);
        }

        private void Update()
        {
            if (timer <= 0f)
            {
                return;
            }

            timer -= Time.deltaTime;

            if (timer > 0f)
            {
                return;
            }

            timer = 0f;
            Apply(false);
        }

        private void Apply(bool clicked)
        {
            if (idleHand != null)
            {
                idleHand.SetActive(!clicked);
            }

            if (clickHand != null)
            {
                clickHand.SetActive(clicked);
            }
        }
    }
}
