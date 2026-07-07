using VXMonster.Core.UI;
using System;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.InputSystem;

namespace VXMonster.Core.Input
{
    public class InputManager : MonoBehaviour, IInputManager
    {
        private static InputManager instance;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void ResetStatic()
        {
            instance = null;
        }

        [Header("References")]
        [SerializeField] HighlightsParentBehavior highlightsParent;
        public HighlightsParentBehavior Highlights => highlightsParent;

        private InputAsset inputAsset;
        public InputAsset InputAsset => inputAsset;

        private InputSave save;

        public InputType ActiveInput { get => save.ActiveInput; private set => save.ActiveInput = value; }
        public Vector2 MovementValue { get; private set; }

        public JoystickBehavior Joystick { get; private set; }

        public event UnityAction<InputType, InputType> onInputChanged;

        private void Awake()
        {
            if(instance != null)
            {
                Destroy(gameObject);
                return;
            }

            instance = this;

            DontDestroyOnLoad(gameObject);

            GameController.RegisterInputManager(this);

            inputAsset = new InputAsset();

            Init();
        }

        private void OnEnable()
        {
            if (inputAsset != null) inputAsset.Enable();
        }

        private void OnDisable()
        {
            if(inputAsset != null) inputAsset.Disable();
        }

        public void Init()
        {
            save = GameController.SaveManager.GetSave<InputSave>("Input");

            if(Gamepad.current != null)
            {
                ActiveInput = InputType.Gamepad;
            } else
            {
                ActiveInput = InputType.UIJoystick;
            }

            inputAsset.GamepadDetection.Detection.performed += GamepadDetection;
        }

        private void Update()
        {
            if (inputAsset == null || save == null) return;

            if (ActiveInput != InputType.Keyboard && Keyboard.current != null && Keyboard.current.wasUpdatedThisFrame && !Keyboard.current.CheckStateIsAtDefaultIgnoringNoise())
            {
                Debug.Log("Switching To Keyboard");

                var prevInput = ActiveInput;
                ActiveInput = InputType.Keyboard;

                if (Joystick != null) Joystick.Disable();

                highlightsParent?.EnableArrows();

                onInputChanged?.Invoke(prevInput, InputType.Keyboard);
            }

            if(ActiveInput != InputType.UIJoystick && 
                (Mouse.current != null && Mouse.current.leftButton.wasPressedThisFrame ||
                ActiveInput == InputType.Gamepad && Gamepad.current == null || 
                Touchscreen.current != null && Touchscreen.current.wasUpdatedThisFrame))
            {
                Debug.Log("Switching To UI Joystick");

                var prevInput = ActiveInput;
                ActiveInput = InputType.UIJoystick;

                if (Joystick != null) Joystick.Enable();

                highlightsParent?.DisableArrows();

                onInputChanged?.Invoke(prevInput, InputType.UIJoystick);
            }

            if (ActiveInput == InputType.UIJoystick && Joystick != null)
            {
                MovementValue = Joystick.Value;
            } else
            {
                MovementValue = inputAsset.Gameplay.Movement.ReadValue<Vector2>();
            } 
        }

        private void GamepadDetection(InputAction.CallbackContext obj)
        {
            if (ActiveInput != InputType.Gamepad)
            {
                Debug.Log("Switching To Gamepad");

                var prevInput = ActiveInput;
                ActiveInput = InputType.Gamepad;

                if (Joystick != null) Joystick.Disable();

                highlightsParent?.EnableArrows();

                onInputChanged?.Invoke(prevInput, InputType.Gamepad);
            }
        }

        public void RegisterJoystick(JoystickBehavior joystick)
        {
            Joystick = joystick;

            if (ActiveInput == InputType.UIJoystick)
            {
                joystick.Enable();
            } else
            {
                joystick.Disable();
            }
        }

        public void RemoveJoystick()
        {
            Joystick = null;
        }
    }

    public enum InputType
    {
        UIJoystick = 1,
        Keyboard = 2,
        Gamepad = 4,
    }
}