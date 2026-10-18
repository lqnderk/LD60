using UnityEngine;

public class ArcadeMachine : MonoBehaviour
{
    public MoneyManager moneyManager;
    public int income = 50;
    public float interval = 5f;

    private float timer;

    private void OnEnable()
    {
        timer = 0f;
    }

    private void Update()
    {
        if (moneyManager == null || interval <= 0f) return;

        timer += Time.deltaTime;

        if (timer < interval) return;

        timer -= interval;
        moneyManager.AddArcadeMoney(income);
    }
}
