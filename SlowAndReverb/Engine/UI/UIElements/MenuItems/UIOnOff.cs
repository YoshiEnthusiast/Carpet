using System;
using System.Collections.Generic;

namespace SlowAndReverb
{
    public class UIOnOff : UILeftRight
    {
        private readonly Dictionary<bool, string> _binaryStateWords = new Dictionary<bool, string>()
        {
            [true] = "ON",
            [false] = "OFF"
        };

        private bool _value;

        public UIOnOff()
        {
            Value = default;
        }

        public bool Value
        {
            get
            {
                return _value;
            }

            set
            {
                _value = value;
                DisplayedItem = _binaryStateWords[_value];
            }
        }

        protected override void UpdateSelected(float deltaTime)
        {
            base.UpdateSelected(deltaTime);

            if (Input.IsPressed(Key.C))
                ToggleValue();
        }

        protected override void OnLeftPressed()
        {
            ToggleValue();
        }

        protected override void OnRightPressed()
        {
            ToggleValue();
        }

        private void ToggleValue()
        {
            Value = !Value;
        }
    }
}
