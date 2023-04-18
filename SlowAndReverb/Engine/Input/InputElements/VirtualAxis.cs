using System;

namespace SlowAndReverb
{
    public class VirtualAxis : InputElement
    {
        private readonly VirtualButton _positiveButton = new VirtualButton();
        private readonly VirtualButton _negativeButton = new VirtualButton();

        private VirtualButton _lastPressedButton;

        private Axis? _axis;

        public float Deadzone { get; set; } = 0.125f;

        public override void Update()
        {
            CheckLastPressedButton(_positiveButton);
            CheckLastPressedButton(_negativeButton);
        }

        public float GetValue()
        {
            if (_axis is not null)
            {
                float axisValue = Input.GetControllerAxis(_axis.Value);

                if (Math.Abs(axisValue) > Deadzone)
                    return axisValue;
            }

            if (!_positiveButton.IsDown() && !_negativeButton.IsDown())
                return 0f;

            if (_lastPressedButton == _positiveButton)
                return 1f;

            return -1f;
        }

        public float GetSign()
        {
            return Math.Sign(GetValue());
        }

        public VirtualAxis SetAxis(Axis axis)
        {
            _axis = axis;

            return this;
        }

        public VirtualAxis AddButtons(Button positive, Button negative)
        {
            _positiveButton.AddButton(positive);
            _negativeButton.AddButton(negative);

            return this;
        }

        public VirtualAxis AddKeys(Key positive, Key negative)
        {
            _positiveButton.AddKey(positive);
            _negativeButton.AddKey(negative);

            return this;
        }

        private void CheckLastPressedButton(VirtualButton button)
        {
            if (button.IsPressed())
                _lastPressedButton = button;
        }
    }
}
