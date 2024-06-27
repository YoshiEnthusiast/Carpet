namespace Carpet
{
    public class Light : Component
    {
        private float _radius;

        public Color Color { get; set; } = Color.White;

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

        public float Rotation { get; set; }
        public float Angle { get; set; } = Maths.TwoPI;
        public float FalloffAngle { get; set; }
        public float StartDistance { get; set; }
        public float StartFade { get; set; }
        public float Volume { get; set; }
        public float ShadowFalloff { get; set; } = 1f;

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
