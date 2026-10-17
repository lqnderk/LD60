using UnityEngine;

public class NewDeviceButton : MonoBehaviour
{
    public RepairManager repairManager;

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space))
        {
            OnMouseDown();
        }
    }

    void OnMouseDown()
    {
        repairManager.NewDevice();
    }
}
