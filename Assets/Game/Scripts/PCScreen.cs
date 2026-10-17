using UnityEngine;

public class PCScreen : MonoBehaviour
{
    public GameObject ComputerScreen;
    public GameObject arrow;

    void Start()
    {
        ComputerScreen.SetActive(false);
    }

    void OnMouseDown()
    {
        ComputerScreen.SetActive(true);
        arrow.SetActive(false);
    }

    void Update()
    {
        if (ComputerScreen.activeSelf && Input.GetKeyDown(KeyCode.Escape))
        {
            ComputerScreen.SetActive(false);
            arrow.SetActive(true);
        }
    }
}