using UnityEngine;

namespace Game.UI
{
    public static class MainScreen
    {
        private static Camera screenCamera;

        public static bool IsActive => screenCamera == null || screenCamera.isActiveAndEnabled;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        private static void Capture()
        {
            screenCamera = Camera.main;
        }
    }
}
