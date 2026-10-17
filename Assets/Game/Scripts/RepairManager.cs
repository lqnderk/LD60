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

    [Header("Manager")]
    [SerializeField] private MoneyManager moneyManager;

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
            RepairDevice repair = newDevice.GetComponent<RepairDevice>();

            repair.requiredClicks = clicks;
            repair.repairManager = this;
            repair.reward = device.reward;
            repair.moneyManager = moneyManager;
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
