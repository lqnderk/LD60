using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public static class SortingStabilizer
{
    private static bool applied;

    public static int TopOrder { get; private set; }

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    private static void Subscribe()
    {
        SceneManager.sceneLoaded -= HandleSceneLoaded;
        SceneManager.sceneLoaded += HandleSceneLoaded;
    }

    private static void HandleSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        applied = false;
        Ensure();
    }

    public static void Ensure()
    {
        if (applied)
        {
            return;
        }

        applied = true;

        Renderer[] renderers = Object.FindObjectsByType<Renderer>(FindObjectsInactive.Include, FindObjectsSortMode.None);
        List<Renderer> ordered = new List<Renderer>(renderers.Length);

        for (int i = 0; i < renderers.Length; i++)
        {
            if (renderers[i] is SpriteRenderer || renderers[i] is MeshRenderer)
            {
                ordered.Add(renderers[i]);
            }
        }

        ordered.Sort(Compare);

        for (int i = 0; i < ordered.Count; i++)
        {
            ordered[i].sortingOrder = i;
        }

        TopOrder = ordered.Count;
    }

    private static int Compare(Renderer first, Renderer second)
    {
        int byOrder = first.sortingOrder.CompareTo(second.sortingOrder);

        if (byOrder != 0)
        {
            return byOrder;
        }

        return string.CompareOrdinal(PathKey(first.transform), PathKey(second.transform));
    }

    private static string PathKey(Transform transform)
    {
        string key = string.Empty;
        Transform current = transform;

        while (current != null)
        {
            key = current.GetSiblingIndex().ToString("D4") + "/" + key;
            current = current.parent;
        }

        return key;
    }
}
