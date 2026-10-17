using UnityEngine;

[CreateAssetMenu(menuName = "Device Type")]
public class DeviceType : ScriptableObject
{
    public string deviceName;
    public int minClick;
    public int maxClick;
    public int reward;
    public GameObject prefab;
}
