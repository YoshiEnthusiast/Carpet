using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SlowAndReverb
{
    public class UINumericLeftRight : UILeftRight
    {
        private int _value;

        public UINumericLeftRight()
        {
            Value = Min;
        }

        public int Value
        {
            get
            {
                return _value;
            }

            set
            {
                _value = Maths.Clamp(value, Min, Max);
                DisplayedItem = _value.ToString();
            }
        }

        public int Min { get; set; }
        public int Max { get; set; } = 5;

        protected override void OnLeftPressed()
        {
            Value--;
        }

        protected override void OnRightPressed()
        {
            Value++;
        }
    }
}
