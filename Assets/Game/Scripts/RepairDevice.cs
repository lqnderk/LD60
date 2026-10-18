using Game.UI;
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
    private SpriteRenderer spriteRenderer;

    public bool IsRepaired => isRepaired;

    private void Awake()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
    }

    public bool PerformClick()
    {
        if (isRepaired)
        {
            return false;
        }

        currentClicks++;
        SpawnScrew();
        ScreenShake.Instance.Shake();
        AudioManager.Instance.PlayRepairClick();

        if (currentClicks >= requiredClicks)
        {
            isRepaired = true;

            if (repairManager != null)
            {
                repairManager.removeDevice();
            }

            if (moneyManager != null)
            {
                moneyManager.AddMoney(reward);
            }

            ScreenEdgeFlash.Instance.Flash();

            return true;
        }

        return false;
    }

    private void SpawnScrew()
    {
        ScrewBurst burst = ScrewBurst.Instance;

        if (burst == null)
        {
            return;
        }

        Vector3 origin = transform.position;

        if (spriteRenderer != null)
        {
            Vector3 extents = spriteRenderer.bounds.extents;
            origin += new Vector3(Random.Range(-extents.x, extents.x), Random.Range(-extents.y, extents.y), 0f) * 0.7f;
        }

        burst.Spawn(origin);
    }

    private void OnMouseDown()
    {
        if (HandAnimator.Instance != null)
        {
            HandAnimator.Instance.Punch();
        }

        PerformClick();
    }
}
