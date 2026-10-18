using UnityEngine;
using TMPro;

public class Target : MonoBehaviour
{
    public MoneyManager moneyManager;
    public GameObject goToPCScreen;
    public int targetAmount = 200;
    public int midTargetAmount = 1000;
    public int ultimateTargetAmount = 20000;

    private int completedStages;
    private bool finalTargetComplete;
    public TMP_Text targetText;

    public bool FirstTargetReached => completedStages >= 1;

    private void Start()
    {
        if (goToPCScreen != null)
        {
            goToPCScreen.SetActive(false);
        }

        UpdateTargetText();
    }

    private void Update()
    {
        if (finalTargetComplete || moneyManager == null) return;

        if (moneyManager.balance < targetAmount) return;

        AdvanceStage();
    }

    private void AdvanceStage()
    {
        completedStages++;

        switch (completedStages)
        {
            case 1:
                targetAmount = midTargetAmount;
                UpdateTargetText();

                if (goToPCScreen != null)
                {
                    goToPCScreen.SetActive(true);
                }
                break;

            case 2:
                targetAmount = ultimateTargetAmount;
                UpdateTargetText();
                break;

            default:
                finalTargetComplete = true;
                break;
        }
    }

    private void UpdateTargetText()
    {
        if (targetText != null)
        {
            targetText.text = "Target $:" + targetAmount;
        }
    }
}
