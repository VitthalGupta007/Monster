using OctoberStudio.UI;
using UnityEngine;
using UnityEngine.Events;

namespace OctoberStudio.Input
{
    public interface IInputManager
    {
        InputType ActiveInput { get; }
        InputAsset InputAsset { get; }

        Vector2 MovementValue { get; }
        JoystickBehavior Joystick { get; }
        HighlightsParentBehavior Highlights { get; }

        event UnityAction<InputType, InputType> onInputChanged;

        void RegisterJoystick(JoystickBehavior joystick);
        void RemoveJoystick();
    }
}