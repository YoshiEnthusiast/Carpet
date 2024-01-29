using System;

namespace Carpet
{
    public class VirtualAxis : InputElement
    {
        private const float PositiveDirection = 1f;
        private const float NegativeDirection = -1f;

        private readonly VirtualButton _positiveButton = new VirtualButton();
        private readonly VirtualButton _negativeButton = new VirtualButton();

        private VirtualButton _lastPressedButton;

        private Axis? _axis;

        public float Deadzone { get; set; } = 0.125f;

        public override void Update(float deltaTime)
        {
            CheckLastPressedButton(_positiveButton);
            CheckLastPressedButton(_negativeButton);
        }

        public float GetValue()
        {
            if (_axis is not null)
            {
                float axisValue = Input.GetControllerAxis(_axis.Value);

                if (Maths.Abs(axisValue) > Deadzone)
                    return axisValue;
            }

            bool positiveIsDown = _positiveButton.IsDown();
            bool negativeIsDown = _negativeButton.IsDown();

            if (positiveIsDown && negativeIsDown)
            {
                if (_lastPressedButton == _positiveButton)
                    return PositiveDirection;

                return NegativeDirection;
            }
            else if (positiveIsDown)
            {
                return PositiveDirection;
            }
            else if (negativeIsDown)
            {
                return NegativeDirection;
            }

            return 0f;
        }

        public float GetSign()
        {
            return Maths.Sign(GetValue());
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
