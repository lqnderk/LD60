using System.Collections.Generic;
using UnityEngine;

public class SortingBand : MonoBehaviour
{
    [SerializeField] private int baseOrder = 20;

    private void Awake()
    {
        ApplyTo(gameObject, baseOrder);
    }

    public static void ApplyTo(GameObject root, int baseOrder)
    {
        var renderers = root.GetComponentsInChildren<Renderer>(true);

        if (renderers.Length == 0)
        {
            return;
        }

        var ordered = new List<Renderer>(renderers);

        for (int i = 0; i < ordered.Count; i++)
        {
            for (int j = i + 1; j < ordered.Count; j++)
            {
                if (ordered[j].sortingOrder < ordered[i].sortingOrder)
                {
                    Renderer swap = ordered[i];
                    ordered[i] = ordered[j];
                    ordered[j] = swap;
                }
            }
        }

        for (int i = 0; i < ordered.Count; i++)
        {
            ordered[i].sortingOrder = baseOrder + i;
        }
    }
}
