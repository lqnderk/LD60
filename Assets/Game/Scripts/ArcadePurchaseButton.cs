using UnityEngine;
using UnityEngine.UI;

public class ArcadePurchaseButton : MonoBehaviour
{
    public MoneyManager moneyManager;
    public GameObject arcadeMachine;
    public int price = 1000;
    public bool hideSelfOnPurchase = true;

    private Button button;
    private bool isPurchased;

    private void Awake()
    {
        button = GetComponent<Button>();
        if (button != null)
        {
            button.onClick.AddListener(Purchase);
        }
    }

    private void Start()
    {
        if (arcadeMachine != null)
        {
            arcadeMachine.SetActive(false);
        }
    }

    private void OnMouseDown()
    {
        if (button == null)
        {
            Purchase();
        }
    }

    public void Purchase()
    {
        if (isPurchased || moneyManager == null) return;

        if (!moneyManager.TrySpendMoney(price)) return;

        isPurchased = true;

        if (arcadeMachine != null)
        {
            arcadeMachine.SetActive(true);
        }

        if (hideSelfOnPurchase)
        {
            gameObject.SetActive(false);
        }
    }
}
