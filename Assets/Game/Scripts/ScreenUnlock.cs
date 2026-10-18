using UnityEngine;

public class ScreenUnlock : MonoBehaviour
{
    public MoneyManager moneyManager;
    public GameObject arrow;
    public int requiredAmount = 1000;

    private bool isUnlocked;

    private void Start()
    {
        if (arrow != null)
        {
            arrow.SetActive(false);
        }
    }

    private void Update()
    {
        if (isUnlocked || moneyManager == null) return;

        if (moneyManager.balance >= requiredAmount)
        {
            isUnlocked = true;

            if (arrow != null)
            {
                arrow.SetActive(true);
            }
        }
    }
}
