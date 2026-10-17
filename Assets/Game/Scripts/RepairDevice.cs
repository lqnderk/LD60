using UnityEngine;

public class RepairDevice : MonoBehaviour
{
    public int currentClicks;
    public int requiredClicks;
    public int reward;

    public RepairManager repairManager;
    public MoneyManager moneyManager;

    private DeviceType deviceType;

    private bool isRepaired = false;

    void OnMouseDown()
    {
        if (isRepaired) return;

        currentClicks++;

        if (currentClicks >= requiredClicks)
        {
            isRepaired = true;

            repairManager.removeDevice();
            moneyManager.AddMoney(reward);
        }
    }
}
