using System.Collections;
using UnityEngine;

public class DeviceMover : MonoBehaviour
{
    public float slideTime = 0.6f;

    public void SlideTo(Vector3 target)
    {
        StartCoroutine(Slide(target));
    }

    IEnumerator Slide(Vector3 target)
    {
        Vector3 from = transform.position;
        float time = 0f;

        while (time < slideTime)
        {
            time += Time.deltaTime;
            float k = Mathf.SmoothStep(0f, 1f, time / slideTime);
            transform.position = Vector3.Lerp(from, target, k);
            yield return null;
        }

        transform.position = target;
    }
}