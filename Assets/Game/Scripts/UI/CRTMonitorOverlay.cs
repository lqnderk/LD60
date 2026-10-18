using UnityEngine;
using UnityEngine.UI;

namespace Game.UI
{
    public class CRTMonitorOverlay : MonoBehaviour
    {
        [SerializeField] private Material crtMaterial;
        [SerializeField] private bool createOverlayAutomatically = true;
        [SerializeField] private int sortingOrder = 10000;

        [Header("CRT Settings")]
        [Range(0f, 1f)] [SerializeField] private float curvature = 0.28f;
        [Range(0.01f, 0.3f)] [SerializeField] private float cornerRoundness = 0.08f;
        [Range(0f, 1f)] [SerializeField] private float scanlineIntensity = 0.35f;
        [SerializeField] private float scanlineCount = 340f;
        [Range(0f, 2f)] [SerializeField] private float vignetteIntensity = 0.55f;
        [Range(0f, 1f)] [SerializeField] private float glassReflection = 0.18f;
        [SerializeField] private Color bezelColor = new Color(0.04f, 0.04f, 0.06f, 1f);

        [Header("Render Texture Distortion")]
        [SerializeField] private bool useRenderTextureDistortion = false;
        [SerializeField] private Camera targetCamera;

        private GameObject overlayObject;
        private Canvas overlayCanvas;
        private Material runtimeMaterial;
        private RenderTexture renderTexture;

        private static readonly int CurvatureProperty = Shader.PropertyToID("_Curvature");
        private static readonly int CornerRoundnessProperty = Shader.PropertyToID("_CornerRoundness");
        private static readonly int ScanlineIntensityProperty = Shader.PropertyToID("_ScanlineIntensity");
        private static readonly int ScanlineCountProperty = Shader.PropertyToID("_ScanlineCount");
        private static readonly int VignetteIntensityProperty = Shader.PropertyToID("_VignetteIntensity");
        private static readonly int GlassReflectionProperty = Shader.PropertyToID("_GlassReflection");
        private static readonly int BezelColorProperty = Shader.PropertyToID("_BezelColor");
        private static readonly int DistortTextureProperty = Shader.PropertyToID("_DistortTexture");

        private void Awake()
        {
            if (createOverlayAutomatically)
            {
                EnsureOverlay();
            }
        }

        private void OnEnable()
        {
            if (overlayObject == null && createOverlayAutomatically)
            {
                EnsureOverlay();
            }

            if (overlayObject != null)
            {
                overlayObject.SetActive(true);
            }

            if (useRenderTextureDistortion && targetCamera != null && renderTexture != null)
            {
                targetCamera.targetTexture = renderTexture;
            }

            UpdateMaterialProperties();
        }

        private void OnDisable()
        {
            if (overlayObject != null)
            {
                overlayObject.SetActive(false);
            }

            if (targetCamera != null && targetCamera.targetTexture == renderTexture)
            {
                targetCamera.targetTexture = null;
            }
        }

        private void Update()
        {
            UpdateMaterialProperties();
        }

        private void EnsureOverlay()
        {
            if (overlayObject != null) return;

            if (runtimeMaterial == null)
            {
                if (crtMaterial != null)
                {
                    runtimeMaterial = new Material(crtMaterial);
                }
                else
                {
                    var shader = Shader.Find("UI/CRTMonitor");
                    if (shader != null)
                    {
                        runtimeMaterial = new Material(shader);
                    }
                }
            }

            overlayObject = new GameObject("CRT_Overlay");
            overlayObject.transform.SetParent(transform, false);

            overlayCanvas = overlayObject.AddComponent<Canvas>();
            overlayCanvas.renderMode = RenderMode.ScreenSpaceOverlay;
            overlayCanvas.sortingOrder = sortingOrder;

            var scaler = overlayObject.AddComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920, 1080);

            var canvasGroup = overlayObject.AddComponent<CanvasGroup>();
            canvasGroup.blocksRaycasts = false;
            canvasGroup.interactable = false;

            var imageObject = new GameObject("CRT_Image");
            imageObject.transform.SetParent(overlayObject.transform, false);

            var rectTransform = imageObject.AddComponent<RectTransform>();
            rectTransform.anchorMin = Vector2.zero;
            rectTransform.anchorMax = Vector2.one;
            rectTransform.offsetMin = Vector2.zero;
            rectTransform.offsetMax = Vector2.zero;

            if (useRenderTextureDistortion && targetCamera != null)
            {
                renderTexture = new RenderTexture(Screen.width, Screen.height, 24);
                targetCamera.targetTexture = renderTexture;

                var rawImage = imageObject.AddComponent<RawImage>();
                rawImage.texture = renderTexture;
                rawImage.material = runtimeMaterial;
                rawImage.raycastTarget = false;

                if (runtimeMaterial != null)
                {
                    runtimeMaterial.SetFloat(DistortTextureProperty, 1f);
                }
            }
            else
            {
                var image = imageObject.AddComponent<Image>();
                image.color = Color.white;
                image.material = runtimeMaterial;
                image.raycastTarget = false;

                if (runtimeMaterial != null)
                {
                    runtimeMaterial.SetFloat(DistortTextureProperty, 0f);
                }
            }

            UpdateMaterialProperties();
        }

        private void UpdateMaterialProperties()
        {
            if (runtimeMaterial == null) return;

            runtimeMaterial.SetFloat(CurvatureProperty, curvature);
            runtimeMaterial.SetFloat(CornerRoundnessProperty, cornerRoundness);
            runtimeMaterial.SetFloat(ScanlineIntensityProperty, scanlineIntensity);
            runtimeMaterial.SetFloat(ScanlineCountProperty, scanlineCount);
            runtimeMaterial.SetFloat(VignetteIntensityProperty, vignetteIntensity);
            runtimeMaterial.SetFloat(GlassReflectionProperty, glassReflection);
            runtimeMaterial.SetColor(BezelColorProperty, bezelColor);
        }

        private void OnDestroy()
        {
            if (renderTexture != null)
            {
                if (targetCamera != null && targetCamera.targetTexture == renderTexture)
                {
                    targetCamera.targetTexture = null;
                }
                renderTexture.Release();
                Destroy(renderTexture);
            }

            if (runtimeMaterial != null)
            {
                Destroy(runtimeMaterial);
            }

            if (overlayObject != null)
            {
                Destroy(overlayObject);
            }
        }
    }
}
