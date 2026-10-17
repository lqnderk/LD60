using System.Collections.Generic;
using UnityEngine;

public class RepairManager : MonoBehaviour
{
    [Header("Devices")]
    [SerializeField] private List<DeviceType> availableDevices;

    [Header("Spawnpoint")] 
    [SerializeField] public Transform spawnPoint;
    [SerializeField] public Transform tablePoint;
    [SerializeField] public Transform removePoint;

    public bool IsDeviceOnTable = false;

    private GameObject newDevice;

    public void NewDevice()
    {
        if (IsDeviceOnTable == false)
        {
            DeviceType device = availableDevices[Random.Range(0, availableDevices.Count)];

            newDevice = Instantiate(device.prefab, spawnPoint.position, Quaternion.identity);

            newDevice.GetComponent<DeviceMover>().SlideTo(tablePoint.position);
            IsDeviceOnTable = true;

            int clicks = Random.Range(device.minClick, device.maxClick + 1);
            newDevice.GetComponent<RepairDevice>().requiredClicks = clicks;
            newDevice.GetComponent<RepairDevice>().repairManager = this;
        }
    }

    void Update()
    {
        if (IsDeviceOnTable == true && Input.GetKeyDown(KeyCode.R))
        {
            removeDevice();    
        }
    }

    public void removeDevice()
    {
            newDevice.GetComponent<DeviceMover>().SlideTo(removePoint.position);
            IsDeviceOnTable = false;
    }
}
