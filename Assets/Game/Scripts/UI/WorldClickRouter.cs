using UnityEngine;

namespace Game.UI
{
    public class WorldClickRouter : MonoBehaviour
    {
        public static int LastHandledFrame { get; private set; } = -1;

        public static void MarkHandled()
        {
            LastHandledFrame = Time.frameCount;
        }

        private static WorldClickRouter instance;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        private static void EnsureInstance()
        {
            if (instance != null)
            {
                return;
            }

            if (FindFirstObjectByType<WorldClickRouter>() != null)
            {
                return;
            }

            var host = new GameObject(nameof(WorldClickRouter));
            DontDestroyOnLoad(host);
            host.AddComponent<WorldClickRouter>();
        }

        private void Awake()
        {
            if (instance != null && instance != this)
            {
                Destroy(gameObject);
                return;
            }

            instance = this;
        }

        private void OnDestroy()
        {
            if (instance == this)
            {
                instance = null;
            }
        }

        private void Update()
        {
            if (Input.GetMouseButtonDown(0))
            {
                Dispatch();
            }
        }

        private static void Dispatch()
        {
            var cameras = Camera.allCameras;
            if (cameras == null || cameras.Length == 0)
            {
                return;
            }

            var behaviours = FindObjectsByType<MonoBehaviour>(FindObjectsSortMode.None);
            if (behaviours == null || behaviours.Length == 0)
            {
                return;
            }

            Vector3 screenPoint = Input.mousePosition;
            IWorldClickTarget best = null;
            int bestLayer = int.MinValue;
            int bestOrder = int.MinValue;

            for (int c = 0; c < cameras.Length; c++)
            {
                var cam = cameras[c];
                if (cam == null)
                {
                    continue;
                }

                Vector3 world = cam.ScreenToWorldPoint(screenPoint);
                Vector2 point = new Vector2(world.x, world.y);

                for (int i = 0; i < behaviours.Length; i++)
                {
                    if (behaviours[i] is not IWorldClickTarget target)
                    {
                        continue;
                    }

                    var shape = behaviours[i].GetComponent<Collider2D>();
                    if (shape == null || !shape.enabled || !shape.gameObject.activeInHierarchy)
                    {
                        continue;
                    }

                    if (!shape.OverlapPoint(point))
                    {
                        continue;
                    }

                    var sprite = behaviours[i].GetComponent<SpriteRenderer>();
                    int layer = sprite != null ? SortingLayer.GetLayerValueFromID(sprite.sortingLayerID) : 0;
                    int order = sprite != null ? sprite.sortingOrder : 0;

                    if (best == null || layer > bestLayer || (layer == bestLayer && order >= bestOrder))
                    {
                        best = target;
                        bestLayer = layer;
                        bestOrder = order;
                    }
                }
            }

            if (best == null)
            {
                return;
            }

            MarkHandled();
            best.OnWorldClick();
        }
    }
}
