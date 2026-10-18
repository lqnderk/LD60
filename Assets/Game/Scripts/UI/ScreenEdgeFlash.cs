using System.Collections;
using UnityEngine;
using UnityEngine.UI;

namespace Game.UI
{
    public class ScreenEdgeFlash : MonoBehaviour
    {
        private static ScreenEdgeFlash instance;

        public static ScreenEdgeFlash Instance
        {
            get
            {
                if (instance == null)
                {
                    instance = FindFirstObjectByType<ScreenEdgeFlash>();

                    if (instance == null)
                    {
                        var host = new GameObject(nameof(ScreenEdgeFlash));
                        instance = host.AddComponent<ScreenEdgeFlash>();
                    }
                }

                return instance;
            }
        }

        [Header("Flash")]
        [SerializeField] private Color flashColor = new Color(0.45f, 1f, 0.55f, 1f);
        [Range(0f, 1f)] [SerializeField] private float peakAlpha = 0.55f;
        [SerializeField] private float duration = 1f;
        [Range(0f, 0.9f)] [SerializeField] private float fadeInShare = 0.35f;

        [Header("Edge Shape")]
        [Range(0f, 1f)] [SerializeField] private float edgeInnerRadius = 0.4f;
        [SerializeField] private int textureSize = 128;
        [SerializeField] private int sortingOrder = 32000;

        private Image flashImage;
        private Sprite edgeSprite;
        private Texture2D edgeTexture;
        private Coroutine activeFlash;

        private void Awake()
        {
            if (instance != null && instance != this)
            {
                Destroy(gameObject);
                return;
            }

            instance = this;
            DontDestroyOnLoad(gameObject);
        }

        private void OnDestroy()
        {
            if (instance == this)
            {
                instance = null;
            }

            if (edgeSprite != null)
            {
                Destroy(edgeSprite);
            }

            if (edgeTexture != null)
            {
                Destroy(edgeTexture);
            }
        }

        public void Flash()
        {
            if (!isActiveAndEnabled || !MainScreen.IsActive)
            {
                return;
            }

            EnsureOverlay();

            if (activeFlash != null)
            {
                StopCoroutine(activeFlash);
            }

            flashImage.enabled = true;
            activeFlash = StartCoroutine(FlashRoutine());
        }

        private IEnumerator FlashRoutine()
        {
            float fadeIn = Mathf.Max(duration * fadeInShare, 0.0001f);
            float fadeOut = Mathf.Max(duration - fadeIn, 0.0001f);

            float elapsed = 0f;
            while (elapsed < fadeIn)
            {
                if (!MainScreen.IsActive)
                {
                    Hide();
                    yield break;
                }

                elapsed += Time.unscaledDeltaTime;
                SetAlpha(peakAlpha * Mathf.SmoothStep(0f, 1f, Mathf.Clamp01(elapsed / fadeIn)));
                yield return null;
            }

            elapsed = 0f;
            while (elapsed < fadeOut)
            {
                if (!MainScreen.IsActive)
                {
                    Hide();
                    yield break;
                }

                elapsed += Time.unscaledDeltaTime;
                SetAlpha(peakAlpha * (1f - Mathf.SmoothStep(0f, 1f, Mathf.Clamp01(elapsed / fadeOut))));
                yield return null;
            }

            Hide();
        }

        private void Hide()
        {
            SetAlpha(0f);
            flashImage.enabled = false;
            activeFlash = null;
        }

        private void SetAlpha(float alpha)
        {
            var color = flashColor;
            color.a = Mathf.Clamp01(alpha) * flashColor.a;
            flashImage.color = color;
        }

        private void EnsureOverlay()
        {
            if (flashImage != null)
            {
                return;
            }

            var canvasObject = new GameObject("EdgeFlashCanvas");
            canvasObject.transform.SetParent(transform, false);

            var canvas = canvasObject.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = sortingOrder;

            var group = canvasObject.AddComponent<CanvasGroup>();
            group.blocksRaycasts = false;
            group.interactable = false;

            var imageObject = new GameObject("EdgeFlashImage");
            imageObject.transform.SetParent(canvasObject.transform, false);

            var rect = imageObject.AddComponent<RectTransform>();
            rect.anchorMin = Vector2.zero;
            rect.anchorMax = Vector2.one;
            rect.offsetMin = Vector2.zero;
            rect.offsetMax = Vector2.zero;

            flashImage = imageObject.AddComponent<Image>();
            flashImage.sprite = BuildEdgeSprite();
            flashImage.raycastTarget = false;
            flashImage.enabled = false;
            flashImage.color = new Color(flashColor.r, flashColor.g, flashColor.b, 0f);
        }

        private Sprite BuildEdgeSprite()
        {
            int size = Mathf.Max(textureSize, 8);
            float center = (size - 1) * 0.5f;
            float maxDistance = Mathf.Sqrt(2f);

            edgeTexture = new Texture2D(size, size, TextureFormat.RGBA32, false);
            edgeTexture.wrapMode = TextureWrapMode.Clamp;
            edgeTexture.filterMode = FilterMode.Bilinear;

            var pixels = new Color32[size * size];

            for (int y = 0; y < size; y++)
            {
                for (int x = 0; x < size; x++)
                {
                    float nx = (x - center) / center;
                    float ny = (y - center) / center;
                    float distance = Mathf.Sqrt(nx * nx + ny * ny) / maxDistance;
                    float edge = Mathf.SmoothStep(0f, 1f, Mathf.InverseLerp(edgeInnerRadius, 1f, distance));
                    pixels[y * size + x] = new Color32(255, 255, 255, (byte)Mathf.RoundToInt(edge * 255f));
                }
            }

            edgeTexture.SetPixels32(pixels);
            edgeTexture.Apply();

            edgeSprite = Sprite.Create(edgeTexture, new Rect(0f, 0f, size, size), new Vector2(0.5f, 0.5f), 100f);
            return edgeSprite;
        }
    }
}
