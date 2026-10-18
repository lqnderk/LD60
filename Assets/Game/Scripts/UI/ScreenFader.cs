using System;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;

namespace Game.UI
{
    public class ScreenFader : MonoBehaviour
    {
        private static ScreenFader instance;

        public static ScreenFader Instance
        {
            get
            {
                if (instance == null)
                {
                    instance = FindFirstObjectByType<ScreenFader>();
                    if (instance == null)
                    {
                        var faderObject = new GameObject("ScreenFader");
                        instance = faderObject.AddComponent<ScreenFader>();
                    }
                }
                return instance;
            }
        }

        [SerializeField] private Material fadeMaterial;
        [SerializeField] private Color fadeColor = new Color(0.06f, 0.06f, 0.09f, 1f);
        [SerializeField] private Color accentColor = new Color(0.95f, 0.3f, 0.45f, 1f);
        [SerializeField] private float defaultDuration = 0.55f;

        private Image fadeImage;
        private Canvas fadeCanvas;
        private Material runtimeMaterial;
        private Coroutine activeTransition;

        private static readonly int ProgressProperty = Shader.PropertyToID("_Progress");
        private static readonly int AspectRatioProperty = Shader.PropertyToID("_AspectRatio");
        private static readonly int ColorProperty = Shader.PropertyToID("_Color");
        private static readonly int AccentColorProperty = Shader.PropertyToID("_AccentColor");

        public bool IsFading => activeTransition != null;

        private void Awake()
        {
            if (instance != null && instance != this)
            {
                Destroy(gameObject);
                return;
            }

            instance = this;
            DontDestroyOnLoad(gameObject);
            EnsureComponents();
            SetProgress(0f);
        }

        private void EnsureComponents()
        {
            fadeCanvas = GetComponentInChildren<Canvas>();
            if (fadeCanvas == null)
            {
                var canvasObject = new GameObject("FaderCanvas");
                canvasObject.transform.SetParent(transform, false);

                fadeCanvas = canvasObject.AddComponent<Canvas>();
                fadeCanvas.renderMode = RenderMode.ScreenSpaceOverlay;
                fadeCanvas.sortingOrder = 32767;

                var scaler = canvasObject.AddComponent<CanvasScaler>();
                scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
                scaler.referenceResolution = new Vector2(1920, 1080);

                canvasObject.AddComponent<GraphicRaycaster>();
            }

            fadeImage = fadeCanvas.GetComponentInChildren<Image>();
            if (fadeImage == null)
            {
                var imageObject = new GameObject("FaderImage");
                imageObject.transform.SetParent(fadeCanvas.transform, false);

                fadeImage = imageObject.AddComponent<Image>();
                var rect = fadeImage.rectTransform;
                rect.anchorMin = Vector2.zero;
                rect.anchorMax = Vector2.one;
                rect.offsetMin = Vector2.zero;
                rect.offsetMax = Vector2.zero;
            }

            if (runtimeMaterial == null)
            {
                if (fadeMaterial != null)
                {
                    runtimeMaterial = new Material(fadeMaterial);
                }
                else
                {
                    Shader shader = Resources.Load<Shader>("Shaders/CircleWipe");

                    if (shader == null)
                    {
                        shader = Shader.Find("UI/CircleWipe");
                    }

                    if (shader != null)
                    {
                        runtimeMaterial = new Material(shader);
                    }
                }
            }

            if (runtimeMaterial != null)
            {
                runtimeMaterial.SetColor(ColorProperty, fadeColor);
                runtimeMaterial.SetColor(AccentColorProperty, accentColor);
                fadeImage.material = runtimeMaterial;
                fadeImage.color = Color.white;
            }
            else
            {
                fadeImage.color = Color.clear;
            }

            fadeImage.raycastTarget = false;
        }

        public void FadeOut(float duration = -1f, Action onComplete = null)
        {
            if (duration < 0f) duration = defaultDuration;
            StartTransition(0f, 1f, duration, onComplete);
        }

        public void FadeIn(float duration = -1f, Action onComplete = null)
        {
            if (duration < 0f) duration = defaultDuration;
            StartTransition(1f, 0f, duration, onComplete);
        }

        public void FadeOutAndIn(float fadeOutDuration = 0.5f, float holdDuration = 0.15f, float fadeInDuration = 0.5f, Action onMiddle = null, Action onComplete = null)
        {
            if (activeTransition != null)
            {
                StopCoroutine(activeTransition);
            }

            activeTransition = StartCoroutine(FadeOutAndInRoutine(fadeOutDuration, holdDuration, fadeInDuration, onMiddle, onComplete));
        }

        private void StartTransition(float from, float to, float duration, Action onComplete)
        {
            if (activeTransition != null)
            {
                StopCoroutine(activeTransition);
            }

            activeTransition = StartCoroutine(TransitionRoutine(from, to, duration, onComplete));
        }

        private IEnumerator TransitionRoutine(float from, float to, float duration, Action onComplete)
        {
            EnsureComponents();
            fadeImage.raycastTarget = to > 0.5f;

            float elapsed = 0f;
            while (elapsed < duration)
            {
                elapsed += Time.unscaledDeltaTime;
                float t = Mathf.Clamp01(elapsed / duration);
                float curvedT = Mathf.SmoothStep(0f, 1f, t);
                SetProgress(Mathf.Lerp(from, to, curvedT));
                yield return null;
            }

            SetProgress(to);
            fadeImage.raycastTarget = to > 0.5f;
            activeTransition = null;
            onComplete?.Invoke();
        }

        private IEnumerator FadeOutAndInRoutine(float fadeOutDuration, float holdDuration, float fadeInDuration, Action onMiddle, Action onComplete)
        {
            EnsureComponents();
            fadeImage.raycastTarget = true;

            float elapsed = 0f;
            while (elapsed < fadeOutDuration)
            {
                elapsed += Time.unscaledDeltaTime;
                float t = Mathf.Clamp01(elapsed / fadeOutDuration);
                SetProgress(Mathf.SmoothStep(0f, 1f, t));
                yield return null;
            }

            SetProgress(1f);
            onMiddle?.Invoke();

            if (holdDuration > 0f)
            {
                yield return new WaitForSecondsRealtime(holdDuration);
            }

            elapsed = 0f;
            while (elapsed < fadeInDuration)
            {
                elapsed += Time.unscaledDeltaTime;
                float t = Mathf.Clamp01(elapsed / fadeInDuration);
                SetProgress(Mathf.SmoothStep(1f, 0f, t));
                yield return null;
            }

            SetProgress(0f);
            fadeImage.raycastTarget = false;
            activeTransition = null;
            onComplete?.Invoke();
        }

        private void SetProgress(float progress)
        {
            if (runtimeMaterial == null) return;

            float aspect = (float)Screen.width / Mathf.Max(Screen.height, 1);
            runtimeMaterial.SetFloat(AspectRatioProperty, aspect);
            runtimeMaterial.SetFloat(ProgressProperty, progress);
        }

        private void OnDestroy()
        {
            if (runtimeMaterial != null)
            {
                Destroy(runtimeMaterial);
            }
        }
    }
}
