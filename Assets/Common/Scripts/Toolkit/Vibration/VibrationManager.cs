using OctoberStudio.Input;
using UnityEngine;
#if UNITY_WEBGL
using UnityEngine.InputSystem.WebGL;
#endif

namespace OctoberStudio.Vibration
{
    public class VibrationManager : MonoBehaviour, IVibrationManager
    {
        private static VibrationManager instance;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void ResetStatic()
        {
            instance = null;
        }

        private VibrationSave save;

        private SimpleVibrationHandler vibrationHandler;

        public bool IsVibrationEnabled { get => save.IsVibrationEnabled; set => save.IsVibrationEnabled = value; }

        private void Awake()
        {
            if(instance != null)
            {
                Destroy(gameObject);
                return;
            }

            instance = this;
            DontDestroyOnLoad(gameObject);

            GameController.RegisterVibrationManager(this);
        }

        public void Start()
        {
            save = GameController.SaveManager.GetSave<VibrationSave>("Vibration");
#if UNITY_EDITOR
            vibrationHandler = new SimpleVibrationHandler(this);
#elif UNITY_IOS
            vibrationHandler = new IOSVibrationHandler(this);
#elif UNITY_ANDROID
            vibrationHandler = new AndroidVibrationHandler(this);
#elif UNITY_WEBGL
            vibrationHandler = new WebGLVibrationHandler(this);
#else
            vibrationHandler = new SimpleVibrationHandler(this);
#endif
        }

        public void Vibrate(float duration, float intensity = 1.0f)
        {
            if (!IsVibrationEnabled) return;

            if (duration <= 0) return;

            if (GameController.InputManager.ActiveInput == InputType.Gamepad)
            {
                vibrationHandler.VibrateGamepad(duration, intensity);
            }
            else
            {
                vibrationHandler.Vibrate(duration, intensity);
            }
        }

        public virtual void LightVibration() => Vibrate(0.08f, 0.4f);
        public virtual void MediumVibration() => Vibrate(0.1f, 0.6f);
        public virtual void StrongVibration() => Vibrate(0.15f, 1f);

        // Making Unity add Vibration permission to android manifest
        protected virtual void TestVibration()
        {
#if UNITY_ANDROID || UNITY_IOS
            Handheld.Vibrate();
#endif
        }
    }
}