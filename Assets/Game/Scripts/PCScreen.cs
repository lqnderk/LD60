using UnityEngine;
using Game.UI;

public class PCScreen : MonoBehaviour
{
    public GameObject ComputerScreen;
    public GameObject arrow;

    private void Start()
    {
        if (ComputerScreen != null)
        {
            ComputerScreen.SetActive(false);
        }
    }

    private void OnMouseDown()
    {
        if (ComputerScreen != null)
        {
            ComputerScreen.SetActive(true);
        }

        if (arrow != null)
        {
            arrow.SetActive(false);
        }
    }

    private void Update()
    {
        if (ComputerScreen != null && ComputerScreen.activeSelf && Input.GetKeyDown(KeyCode.Escape))
        {
            if (UIWindow.HasActiveWindows)
            {
                return;
            }

            ComputerScreen.SetActive(false);

            if (arrow != null)
            {
                arrow.SetActive(true);
            }
        }
    }
}