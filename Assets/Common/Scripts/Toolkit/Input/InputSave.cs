using VXMonster.Core.Input;
using VXMonster.Core.Save;
using UnityEngine;

namespace VXMonster.Core
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