using UnityEngine;

public class RepairDevice : MonoBehaviour
{
    public int currentClicks;
    public int requiredClicks;

    public RepairManager repairManager;

    private DeviceType deviceType;

    void OnMouseDown()
    {
        currentClicks++;

        if (currentClicks >= requiredClicks)
        {
            repairManager.removeDevice();
        }
    }
}
