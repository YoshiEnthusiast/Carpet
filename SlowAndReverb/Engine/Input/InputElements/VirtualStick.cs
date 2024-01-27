namespace Carpet
{
    public class VirtualStick : InputElement
    {
        private readonly VirtualAxis _virtualXAxis = new VirtualAxis();
        private readonly VirtualAxis _virtualYAxis = new VirtualAxis();

        public override void Update()
        {
            _virtualXAxis.Update();
            _virtualYAxis.Update();
        }

        public float GetAngle()
        {
            float x = GetXValue();
            float y = GetYValue();

            return Maths.Atan2(y, x);
        }

        public float GetXValue()
        {
            return _virtualXAxis.GetValue();
        }

        public float GetYValue()
        {
            return _virtualYAxis.GetValue();
        }

        public float GetXSign()
        {
            return _virtualXAxis.GetSign();
        }

        public float GetYSign()
        {
            return _virtualYAxis.GetSign();
        } 

        public VirtualStick SetAxes(Axis x, Axis y)
        {
            _virtualXAxis.SetAxis(x);
            _virtualYAxis.SetAxis(y);

            return this;
        }

        public VirtualStick AddButtons(Button left, Button right, Button up, Button down)
        {
            _virtualXAxis.AddButtons(right, left);
            _virtualYAxis.AddButtons(down, up);

            return this;
        }

        public VirtualStick AddKeys(Key left, Key right, Key up, Key down)
        {
            _virtualXAxis.AddKeys(right, left);
            _virtualYAxis.AddKeys(down, up);

            return this;
        }
    }
}
