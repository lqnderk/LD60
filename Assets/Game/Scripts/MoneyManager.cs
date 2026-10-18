using UnityEngine;
using TMPro;

public class MoneyManager : MonoBehaviour
{
    public TMP_Text money;
    public int balance;
    public float incomeMultiplier = 1f;
    public float arcadeIncomeMultiplier = 1f;

    private void Start()
    {
        UpdateUI();
    }

    public void AddMoney(int amount)
    {
        AddFlatMoney(Mathf.RoundToInt(amount * incomeMultiplier));
    }

    public void AddFlatMoney(int amount)
    {
        balance += amount;
        UpdateUI();
    }

    public void AddArcadeMoney(int amount)
    {
        AddFlatMoney(Mathf.RoundToInt(amount * arcadeIncomeMultiplier));
    }

    public bool TrySpendMoney(int amount)
    {
        if (balance >= amount)
        {
            balance -= amount;
            UpdateUI();
            return true;
        }

        return false;
    }

    public void UpdateUI()
    {
        if (money != null)
        {
            money.text = "Balance $:" + balance;
        }
    }
}
