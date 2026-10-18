using Game.UI;
using UnityEngine;

public class ShopItem : MonoBehaviour, IWorldClickTarget
{
    public enum UpgradeType
    {
        Income,
        AutoRepair,
        ArcadeProfit
    }

    public ShopManager shopManager;
    public UpgradeType upgradeType = UpgradeType.Income;
    public int level = 1;

    private void OnMouseDown()
    {
        if (WorldClickRouter.LastHandledFrame == Time.frameCount)
        {
            return;
        }

        WorldClickRouter.MarkHandled();
        OnWorldClick();
    }

    public void OnWorldClick()
    {
        if (shopManager == null)
        {
            return;
        }

        switch (upgradeType)
        {
            case UpgradeType.Income:
                shopManager.TryBuyIncomeUpgrade(level);
                break;
            case UpgradeType.AutoRepair:
                shopManager.TryBuyAutoRepairUpgrade(level);
                break;
            default:
                shopManager.TryBuyArcadeUpgrade(level);
                break;
        }
    }
}
