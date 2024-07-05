namespace Carpet
{
    public class BloomPoint : Component
    {
        private float _radius;

        public float Radius
        {
            get
            {
                return _radius;
            }

            set
            {
                _radius = Maths.Floor(value);
            }
        }

        public Color Color { get; set; } = Color.White;
        public float Volume { get; set; }

        public Rectangle Bounds
        {
            get
            {
                Vector2 flooredPosition = Position.Floor();

                return Rectangle.FromCircle(flooredPosition, Radius);
            }
        }
    }
}
