using UnityEngine;

public class Target : MonoBehaviour
{
    public MoneyManager moneyManager;
    public GameObject goToPCScreen;
    public int targetAmount = 200;

    private bool firstTargetComplete;

    private void Start()
    {
        if (goToPCScreen != null)
        {
            goToPCScreen.SetActive(false);
        }
    }

    private void Update()
    {
        if (firstTargetComplete || moneyManager == null) return;

        if (moneyManager.balance >= targetAmount)
        {
            firstTargetComplete = true;
            if (goToPCScreen != null)
            {
                goToPCScreen.SetActive(true);
            }
        }
    }
}

