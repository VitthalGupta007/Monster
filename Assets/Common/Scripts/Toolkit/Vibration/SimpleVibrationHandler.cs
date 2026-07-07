using System.Collections;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.InputSystem;

namespace VXMonster.Core.Vibration
{
    public class SimpleVibrationHandler
    {
        protected VibrationManager manager;
        
        private Coroutine gamepadVibrationCoroutine;

        public SimpleVibrationHandler(VibrationManager manager)
        {
            this.manager = manager;
        }

        public virtual bool Vibrate(float duration, float intensity)
        {
            return true;
        }

        public virtual bool VibrateGamepad(float duration, float intensity)
        {
            if (Gamepad.current == null) return false;

            Gamepad.current.SetMotorSpeeds(intensity, intensity);

            if (gamepadVibrationCoroutine != null) manager.StopCoroutine(gamepadVibrationCoroutine);

            gamepadVibrationCoroutine = manager.StartCoroutine(DoAfter(duration, OnGamepadVibrationEnded));

            return true;
        }

        protected virtual IEnumerator DoAfter(float duration, UnityAction action)
        {
            yield return new WaitForSecondsRealtime(duration);

            action?.Invoke();
        }

        protected virtual void OnGamepadVibrationEnded()
        {
            if (Gamepad.current != null) Gamepad.current.SetMotorSpeeds(0, 0);

            gamepadVibrationCoroutine = null;
        }
    }
}