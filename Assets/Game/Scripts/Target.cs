using UnityEngine;

public class Target : MonoBehaviour
{
    public MoneyManager moneyManager;
    private bool firstTargetComplete = false;

    void Update()
    {
        if (!firstTargetComplete && moneyManager.balance >= 200)
        {
            Debug.Log("Первая цель завершена");
            firstTargetComplete = true;
        }
    }
}
