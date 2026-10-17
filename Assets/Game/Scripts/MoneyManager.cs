using UnityEngine;
using TMPro;

public class MoneyManager : MonoBehaviour
{
    public TMP_Text money;
    public int balance;

    public void AddMoney(int amount)
    {
        balance += amount;
        money.text = "Balance $:" + balance;
    }
}
