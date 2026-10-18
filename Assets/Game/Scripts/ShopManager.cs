using Game.UI;
using UnityEngine;

public class ShopManager : MonoBehaviour
{
    [Header("Income Upgrade Prices")]
    [SerializeField] private int incomePriceLevel1 = 100;
    [SerializeField] private int incomePriceLevel2 = 250;
    [SerializeField] private int incomePriceLevel3 = 500;

    [Header("Auto-Repair Upgrade Prices")]
    [SerializeField] private int autoRepairPriceLevel1 = 150;
    [SerializeField] private int autoRepairPriceLevel2 = 350;
    [SerializeField] private int autoRepairPriceLevel3 = 700;

    [Header("Arcade Profit Upgrade Prices")]
    [SerializeField] private int arcadePriceLevel1 = 1500;
    [SerializeField] private int arcadePriceLevel2 = 4000;
    [SerializeField] private int arcadePriceLevel3 = 10000;

    [Header("Income Upgrade Items")]
    [SerializeField] private GameObject incomeItemLevel1;
    [SerializeField] private GameObject incomeItemLevel2;
    [SerializeField] private GameObject incomeItemLevel3;

    [Header("Auto-Repair Upgrade Items")]
    [SerializeField] private GameObject autoRepairItemLevel1;
    [SerializeField] private GameObject autoRepairItemLevel2;
    [SerializeField] private GameObject autoRepairItemLevel3;

    [Header("Arcade Profit Upgrade Items")]
    [SerializeField] private GameObject arcadeItemLevel1;
    [SerializeField] private GameObject arcadeItemLevel2;
    [SerializeField] private GameObject arcadeItemLevel3;

    [Header("Income Upgrade Texts")]
    [SerializeField] private GameObject incomeTextLevel1;
    [SerializeField] private GameObject incomeTextLevel2;
    [SerializeField] private GameObject incomeTextLevel3;

    [Header("Auto-Repair Upgrade Texts")]
    [SerializeField] private GameObject autoRepairTextLevel1;
    [SerializeField] private GameObject autoRepairTextLevel2;
    [SerializeField] private GameObject autoRepairTextLevel3;

    [Header("Arcade Profit Upgrade Texts")]
    [SerializeField] private GameObject arcadeTextLevel1;
    [SerializeField] private GameObject arcadeTextLevel2;
    [SerializeField] private GameObject arcadeTextLevel3;

    [Header("Managers")]
    [SerializeField] private MoneyManager moneyManager;
    [SerializeField] private RepairManager repairManager;
    [SerializeField] private MessageSpawner messageSpawner;

    private int incomeLevel = 0;
    private int autoRepairLevel = 0;
    private int arcadeLevel = 0;
    private int lastPurchaseFrame = -1;
    private float autoClickTimer = 0f;

    public int IncomeLevel => incomeLevel;
    public int AutoRepairLevel => autoRepairLevel;
    public int ArcadeLevel => arcadeLevel;

    private void Awake()
    {
        if (moneyManager == null)
        {
            moneyManager = FindFirstObjectByType<MoneyManager>();
        }

        if (repairManager == null)
        {
            repairManager = FindFirstObjectByType<RepairManager>();
        }

        if (messageSpawner == null)
        {
            messageSpawner = FindFirstObjectByType<MessageSpawner>();
        }
    }

    private void Start()
    {
        ApplyIncomeUpgrade();
        ApplyArcadeUpgrade();
        UpdateButtonsUI();
    }

    private void Update()
    {
        UpdateAutoRepair();
    }

    public void BuyIncomeLevel1() => TryBuyIncomeUpgrade(1);
    public void BuyIncomeLevel2() => TryBuyIncomeUpgrade(2);
    public void BuyIncomeLevel3() => TryBuyIncomeUpgrade(3);

    public void BuyAutoRepairLevel1() => TryBuyAutoRepairUpgrade(1);
    public void BuyAutoRepairLevel2() => TryBuyAutoRepairUpgrade(2);
    public void BuyAutoRepairLevel3() => TryBuyAutoRepairUpgrade(3);

    public void BuyArcadeLevel1() => TryBuyArcadeUpgrade(1);
    public void BuyArcadeLevel2() => TryBuyArcadeUpgrade(2);
    public void BuyArcadeLevel3() => TryBuyArcadeUpgrade(3);

    public void TryBuyIncomeUpgrade(int targetLevel)
    {
        if (lastPurchaseFrame == Time.frameCount)
        {
            return;
        }

        if (incomeLevel != targetLevel - 1)
        {
            return;
        }

        int price = GetIncomePrice(targetLevel);
        if (moneyManager == null || !moneyManager.TrySpendMoney(price))
        {
            return;
        }

        incomeLevel = targetLevel;
        lastPurchaseFrame = Time.frameCount;
        ApplyIncomeUpgrade();
        UpdateButtonsUI();
    }

    public void TryBuyAutoRepairUpgrade(int targetLevel)
    {
        if (lastPurchaseFrame == Time.frameCount)
        {
            return;
        }

        if (autoRepairLevel != targetLevel - 1)
        {
            return;
        }

        int price = GetAutoRepairPrice(targetLevel);
        if (moneyManager == null || !moneyManager.TrySpendMoney(price))
        {
            return;
        }

        autoRepairLevel = targetLevel;
        lastPurchaseFrame = Time.frameCount;
        autoClickTimer = 0f;
        UpdateButtonsUI();
    }

    public void TryBuyArcadeUpgrade(int targetLevel)
    {
        if (lastPurchaseFrame == Time.frameCount)
        {
            return;
        }

        if (arcadeLevel != targetLevel - 1)
        {
            return;
        }

        int price = GetArcadePrice(targetLevel);
        if (moneyManager == null || !moneyManager.TrySpendMoney(price))
        {
            return;
        }

        arcadeLevel = targetLevel;
        lastPurchaseFrame = Time.frameCount;
        ApplyArcadeUpgrade();
        UpdateButtonsUI();
    }

    private void ApplyIncomeUpgrade()
    {
        if (moneyManager == null)
        {
            return;
        }

        switch (incomeLevel)
        {
            case 1:
                moneyManager.incomeMultiplier = 1.2f;
                break;
            case 2:
                moneyManager.incomeMultiplier = 1.5f;
                break;
            case 3:
                moneyManager.incomeMultiplier = 1.9f;
                break;
            default:
                moneyManager.incomeMultiplier = 1.0f;
                break;
        }
    }

    private void ApplyArcadeUpgrade()
    {
        if (moneyManager == null)
        {
            return;
        }

        switch (arcadeLevel)
        {
            case 1:
                moneyManager.arcadeIncomeMultiplier = 1.5f;
                break;
            case 2:
                moneyManager.arcadeIncomeMultiplier = 2f;
                break;
            case 3:
                moneyManager.arcadeIncomeMultiplier = 3f;
                break;
            default:
                moneyManager.arcadeIncomeMultiplier = 1f;
                break;
        }
    }

    private void UpdateButtonsUI()
    {
        bool income1 = incomeLevel == 0;
        bool income2 = incomeLevel == 1;
        bool income3 = incomeLevel == 2;

        SetItemActive(incomeItemLevel1, income1);
        SetItemActive(incomeItemLevel2, income2);
        SetItemActive(incomeItemLevel3, income3);

        SetItemActive(incomeTextLevel1, income1);
        SetItemActive(incomeTextLevel2, income2);
        SetItemActive(incomeTextLevel3, income3);

        bool autoRepair1 = autoRepairLevel == 0;
        bool autoRepair2 = autoRepairLevel == 1;
        bool autoRepair3 = autoRepairLevel == 2;

        SetItemActive(autoRepairItemLevel1, autoRepair1);
        SetItemActive(autoRepairItemLevel2, autoRepair2);
        SetItemActive(autoRepairItemLevel3, autoRepair3);

        SetItemActive(autoRepairTextLevel1, autoRepair1);
        SetItemActive(autoRepairTextLevel2, autoRepair2);
        SetItemActive(autoRepairTextLevel3, autoRepair3);

        bool arcade1 = arcadeLevel == 0;
        bool arcade2 = arcadeLevel == 1;
        bool arcade3 = arcadeLevel == 2;

        SetItemActive(arcadeItemLevel1, arcade1);
        SetItemActive(arcadeItemLevel2, arcade2);
        SetItemActive(arcadeItemLevel3, arcade3);

        SetItemActive(arcadeTextLevel1, arcade1);
        SetItemActive(arcadeTextLevel2, arcade2);
        SetItemActive(arcadeTextLevel3, arcade3);
    }

    private void SetItemActive(GameObject item, bool active)
    {
        if (item != null)
        {
            item.SetActive(active);
        }
    }

    private void UpdateAutoRepair()
    {
        if (autoRepairLevel <= 0)
        {
            return;
        }

        if (messageSpawner != null && messageSpawner.IsSpamming)
        {
            autoClickTimer = 0f;
            return;
        }

        if (repairManager == null || !repairManager.IsDeviceOnTable || repairManager.CurrentRepairDevice == null)
        {
            autoClickTimer = 0f;
            return;
        }

        var currentDevice = repairManager.CurrentRepairDevice;
        if (currentDevice == null || currentDevice.IsRepaired)
        {
            return;
        }

        float interval = 1f / autoRepairLevel;
        autoClickTimer += Time.deltaTime;

        if (autoClickTimer >= interval)
        {
            autoClickTimer = 0f;
            currentDevice.PerformClick();
        }
    }

    public int GetIncomePrice(int level)
    {
        switch (level)
        {
            case 1:
                return incomePriceLevel1;
            case 2:
                return incomePriceLevel2;
            case 3:
                return incomePriceLevel3;
            default:
                return 0;
        }
    }

    public int GetAutoRepairPrice(int level)
    {
        switch (level)
        {
            case 1:
                return autoRepairPriceLevel1;
            case 2:
                return autoRepairPriceLevel2;
            case 3:
                return autoRepairPriceLevel3;
            default:
                return 0;
        }
    }

    public int GetArcadePrice(int level)
    {
        switch (level)
        {
            case 1:
                return arcadePriceLevel1;
            case 2:
                return arcadePriceLevel2;
            case 3:
                return arcadePriceLevel3;
            default:
                return 0;
        }
    }
}
