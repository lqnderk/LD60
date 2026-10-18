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

    [Header("Auto New Device")]
    [SerializeField] private int autoNewDeviceAmount = 1000;
    [SerializeField] private float newDeviceDelay = 0.8f;

    public bool IsDeviceOnTable = false;
    public RepairDevice CurrentRepairDevice { get; private set; }

    private GameObject newDevice;
    private bool autoNewDevice;
    private float newDeviceTimer;

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

            CurrentRepairDevice = repair;
        }
    }

    void Update()
    {
        if (IsDeviceOnTable == true && Input.GetKeyDown(KeyCode.R))
        {
            removeDevice();    
        }

        UpdateAutoNewDevice();
    }

    private void UpdateAutoNewDevice()
    {
        if (autoNewDevice == false)
        {
            if (moneyManager != null && moneyManager.balance >= autoNewDeviceAmount)
            {
                autoNewDevice = true;
            }

            return;
        }

        if (IsDeviceOnTable == true)
        {
            newDeviceTimer = 0f;
            return;
        }

        newDeviceTimer += Time.deltaTime;

        if (newDeviceTimer >= newDeviceDelay)
        {
            newDeviceTimer = 0f;
            NewDevice();
        }
    }

    public void removeDevice()
    {
        if (newDevice != null)
        {
            var mover = newDevice.GetComponent<DeviceMover>();
            if (mover != null)
            {
                mover.SlideTo(removePoint.position);
            }
        }

        IsDeviceOnTable = false;
        CurrentRepairDevice = null;
    }
}
