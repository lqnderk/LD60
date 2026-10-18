using System.Collections.Generic;
using UnityEngine;

namespace Game.UI
{
    public class ScreenShake : MonoBehaviour
    {
        private static ScreenShake instance;

        public static ScreenShake Instance
        {
            get
            {
                if (instance == null)
                {
                    instance = FindFirstObjectByType<ScreenShake>();

                    if (instance == null)
                    {
                        var host = new GameObject(nameof(ScreenShake));
                        instance = host.AddComponent<ScreenShake>();
                    }
                }

                return instance;
            }
        }

        [SerializeField] private float duration = 0.22f;
        [SerializeField] private float magnitude = 0.07f;
        [SerializeField] private float frequency = 34f;
        [SerializeField] private float rotation = 0.35f;
        [SerializeField] private float noiseSeed = 0.37f;

        private readonly List<Camera> cameras = new List<Camera>();
        private Vector3 appliedOffset;
        private float appliedAngle;
        private float elapsed;
        private bool isShaking;

        private void Awake()
        {
            if (instance != null && instance != this)
            {
                Destroy(gameObject);
                return;
            }

            instance = this;
            DontDestroyOnLoad(gameObject);
        }

        private void OnDestroy()
        {
            if (instance == this)
            {
                instance = null;
            }
        }

        public void Shake()
        {
            if (!isActiveAndEnabled || !MainScreen.IsActive)
            {
                return;
            }

            if (!isShaking)
            {
                cameras.Clear();
                cameras.AddRange(Camera.allCameras);
            }

            elapsed = 0f;
            isShaking = true;
        }

        private void LateUpdate()
        {
            if (!isShaking)
            {
                return;
            }

            if (!MainScreen.IsActive)
            {
                Stop();
                return;
            }

            elapsed += Time.deltaTime;

            float progress = Mathf.Clamp01(elapsed / Mathf.Max(duration, 0.0001f));
            float falloff = 1f - progress;
            float amount = magnitude * falloff * falloff;
            float noiseTime = Time.time * frequency;

            var offset = new Vector3(
                (Mathf.PerlinNoise(noiseSeed, noiseTime) * 2f - 1f) * amount,
                (Mathf.PerlinNoise(noiseTime, noiseSeed) * 2f - 1f) * amount,
                0f);

            float angle = (Mathf.PerlinNoise(noiseSeed * 2f, noiseTime) * 2f - 1f) * rotation * falloff;

            MoveCameras(offset - appliedOffset, angle - appliedAngle);

            appliedOffset = offset;
            appliedAngle = angle;

            if (progress >= 1f)
            {
                Stop();
            }
        }

        private void Stop()
        {
            MoveCameras(-appliedOffset, -appliedAngle);

            isShaking = false;
            cameras.Clear();
            appliedOffset = Vector3.zero;
            appliedAngle = 0f;
        }

        private void MoveCameras(Vector3 offsetDelta, float angleDelta)
        {
            for (int i = 0; i < cameras.Count; i++)
            {
                Camera camera = cameras[i];

                if (camera == null)
                {
                    continue;
                }

                Transform cameraTransform = camera.transform;
                cameraTransform.position += offsetDelta;
                cameraTransform.rotation *= Quaternion.Euler(0f, 0f, angleDelta);
            }
        }
    }
}
