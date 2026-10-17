using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using Game.UI;

public class CameraSwitchButton : MonoBehaviour, IPointerClickHandler
{
    public GameObject fromCamera;
    public GameObject toCamera;
    public GameObject objectToActivate;
    public bool hideSelfOnSwitch = true;
    public float fadeOutDuration = 0.5f;
    public float holdDuration = 0.1f;
    public float fadeInDuration = 0.5f;

    private Button button;
    private bool isTransitioning;

    private void Awake()
    {
        button = GetComponent<Button>();
        if (button != null)
        {
            button.onClick.AddListener(SwitchCamera);
        }
    }

    private void Start()
    {
        if (fromCamera == null && Camera.main != null)
        {
            fromCamera = Camera.main.gameObject;
        }
    }

    private void OnEnable()
    {
        isTransitioning = false;
    }

    private void OnMouseDown()
    {
        if (button == null)
        {
            SwitchCamera();
        }
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        if (button == null)
        {
            SwitchCamera();
        }
    }

    public void SwitchCamera()
    {
        if (isTransitioning || (ScreenFader.Instance != null && ScreenFader.Instance.IsFading)) return;
        isTransitioning = true;

        ScreenFader.Instance.FadeOutAndIn(fadeOutDuration, holdDuration, fadeInDuration, () =>
        {
            if (fromCamera != null) fromCamera.SetActive(false);
            if (toCamera != null) toCamera.SetActive(true);
            if (objectToActivate != null) objectToActivate.SetActive(true);
            if (hideSelfOnSwitch) gameObject.SetActive(false);
        }, () =>
        {
            isTransitioning = false;
        });
    }
}
