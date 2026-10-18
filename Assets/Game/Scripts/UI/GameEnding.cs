using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace Game.UI
{
    public class GameEnding : MonoBehaviour
    {
        [SerializeField] private int goalAmount = 20000;
        [SerializeField] private string endingMessage = "Damn... my old PC can't even handle this stupid game. I'd better go for a walk...";
        [SerializeField] private string endingSignature = "game by lypth";
        [SerializeField] private float restartDelay = 15f;
        [SerializeField] private float fadeDuration = 1f;
        [SerializeField] private TMP_FontAsset font;

        private MoneyManager moneyManager;
        private Target target;
        private Image blackout;
        private TMP_Text messageText;
        private TMP_Text signatureText;
        private bool triggered;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        private static void Bootstrap()
        {
            if (FindFirstObjectByType<GameEnding>() != null)
            {
                return;
            }

            GameObject host = new GameObject(nameof(GameEnding));
            DontDestroyOnLoad(host);
            host.AddComponent<GameEnding>();
        }

        private void OnEnable()
        {
            SceneManager.sceneLoaded += HandleSceneLoaded;
        }

        private void OnDisable()
        {
            SceneManager.sceneLoaded -= HandleSceneLoaded;
        }

        private void Start()
        {
            SyncTarget();
        }

        private void Update()
        {
            if (triggered)
            {
                return;
            }

            if (moneyManager == null)
            {
                moneyManager = FindFirstObjectByType<MoneyManager>();
            }

            if (target == null)
            {
                SyncTarget();
            }

            if (moneyManager == null || moneyManager.balance < goalAmount)
            {
                return;
            }

            triggered = true;
            StartCoroutine(PlayEnding());
        }

        private void SyncTarget()
        {
            target = FindFirstObjectByType<Target>();

            if (target != null)
            {
                target.ultimateTargetAmount = goalAmount;
            }
        }

        private IEnumerator PlayEnding()
        {
            float previousVolume = AudioListener.volume;

            AudioListener.volume = 0f;
            Time.timeScale = 0f;

            BuildOverlay();

            float elapsed = 0f;

            while (elapsed < fadeDuration)
            {
                elapsed += Time.unscaledDeltaTime;
                ApplyAlpha(Mathf.Clamp01(elapsed / fadeDuration));
                yield return null;
            }

            ApplyAlpha(1f);

            yield return new WaitForSecondsRealtime(Mathf.Max(0f, restartDelay - fadeDuration));

            Time.timeScale = 1f;
            AudioListener.volume = previousVolume;

            if (AudioManager.Instance != null)
            {
                Destroy(AudioManager.Instance.gameObject);
            }

            yield return null;

            SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
        }

        private void BuildOverlay()
        {
            font = ResolveFont();

            GameObject canvasObject = new GameObject("Ending Overlay");

            Canvas canvas = canvasObject.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = 32600;

            CanvasScaler scaler = canvasObject.AddComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920f, 1080f);
            scaler.matchWidthOrHeight = 0.5f;

            GameObject blackoutObject = new GameObject("Blackout");
            blackoutObject.transform.SetParent(canvasObject.transform, false);

            RectTransform blackoutRect = blackoutObject.AddComponent<RectTransform>();
            blackoutRect.anchorMin = Vector2.zero;
            blackoutRect.anchorMax = Vector2.one;
            blackoutRect.offsetMin = Vector2.zero;
            blackoutRect.offsetMax = Vector2.zero;

            blackout = blackoutObject.AddComponent<Image>();
            blackout.color = new Color(0f, 0f, 0f, 0f);
            blackout.raycastTarget = true;

            messageText = CreateText(canvasObject.transform, "Ending Message", 46f, new Vector2(0f, 40f), new Vector2(1440f, 420f));
            messageText.text = endingMessage;

            signatureText = CreateText(canvasObject.transform, "Ending Signature", 26f, new Vector2(0f, -260f), new Vector2(1440f, 80f));
            signatureText.text = endingSignature;
        }

        private TMP_Text CreateText(Transform parent, string name, float size, Vector2 position, Vector2 dimensions)
        {
            GameObject textObject = new GameObject(name);
            textObject.transform.SetParent(parent, false);

            RectTransform rect = textObject.AddComponent<RectTransform>();
            rect.anchorMin = new Vector2(0.5f, 0.5f);
            rect.anchorMax = new Vector2(0.5f, 0.5f);
            rect.pivot = new Vector2(0.5f, 0.5f);
            rect.anchoredPosition = position;
            rect.sizeDelta = dimensions;

            TextMeshProUGUI text = textObject.AddComponent<TextMeshProUGUI>();
            text.font = font;
            text.fontSize = size;
            text.enableAutoSizing = true;
            text.fontSizeMin = size * 0.6f;
            text.fontSizeMax = size;
            text.alignment = TextAlignmentOptions.Center;
            text.textWrappingMode = TextWrappingModes.Normal;
            text.color = new Color(1f, 1f, 1f, 0f);
            text.raycastTarget = false;

            return text;
        }

        private TMP_FontAsset ResolveFont()
        {
            if (font != null)
            {
                return font;
            }

            TMP_Text[] texts = FindObjectsByType<TMP_Text>(FindObjectsInactive.Include, FindObjectsSortMode.None);

            for (int i = 0; i < texts.Length; i++)
            {
                if (texts[i] != null && texts[i].font != null)
                {
                    return texts[i].font;
                }
            }

            return TMP_Settings.defaultFontAsset;
        }

        private void ApplyAlpha(float progress)
        {
            if (blackout != null)
            {
                Color color = blackout.color;
                color.a = progress;
                blackout.color = color;
            }

            float textAlpha = Mathf.Clamp01((progress - 0.4f) / 0.6f);

            if (messageText != null)
            {
                Color color = messageText.color;
                color.a = textAlpha;
                messageText.color = color;
            }

            if (signatureText != null)
            {
                Color color = signatureText.color;
                color.a = textAlpha * 0.6f;
                signatureText.color = color;
            }
        }

        private void HandleSceneLoaded(Scene scene, LoadSceneMode mode)
        {
            triggered = false;
            moneyManager = null;
            target = null;
            blackout = null;
            messageText = null;
            signatureText = null;
        }
    }
}
