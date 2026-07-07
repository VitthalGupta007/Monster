using UnityEngine;
using UnityEngine.UI;

namespace OctoberStudio.UI
{
    [RequireComponent(typeof(CanvasScaler))]
    public class ScalerHelper : MonoBehaviour
    {
        private void Awake()
        {
            GetComponent<CanvasScaler>().matchWidthOrHeight = IsWideScreen() ? 1 : 0;
        }

        public static bool IsWideScreen()
        {
#if UNITY_IOS
            return UnityEngine.iOS.Device.generation.ToString().Contains("iPad");
#else
            return Camera.main.aspect > (9f / 18f);
#endif
        }
    }

}
