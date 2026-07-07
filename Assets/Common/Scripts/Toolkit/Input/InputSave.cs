using OctoberStudio.Input;
using OctoberStudio.Save;
using UnityEngine;

namespace OctoberStudio
{
    public class InputSave : ISave
    {
        [SerializeField] InputType activeInput;

        public InputType ActiveInput { get => activeInput; set => activeInput = value; }

        public void Flush()
        {

        }
    }
}