using UnityEngine;

public class NewDeviceButton : MonoBehaviour
{
    public RepairManager repairManager;

    void OnMouseDown()
    {
        repairManager.NewDevice();
    }
}
