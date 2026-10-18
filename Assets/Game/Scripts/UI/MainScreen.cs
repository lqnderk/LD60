using UnityEngine;
using UnityEngine.SceneManagement;

namespace Game.UI
{
    public static class MainScreen
    {
        private static Camera screenCamera;

        public static bool IsActive => screenCamera == null || screenCamera.isActiveAndEnabled;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        private static void Subscribe()
        {
            SceneManager.sceneLoaded -= HandleSceneLoaded;
            SceneManager.sceneLoaded += HandleSceneLoaded;
        }

        private static void HandleSceneLoaded(Scene scene, LoadSceneMode mode)
        {
            Capture();
        }

        private static void Capture()
        {
            screenCamera = Camera.main;
        }
    }
}
