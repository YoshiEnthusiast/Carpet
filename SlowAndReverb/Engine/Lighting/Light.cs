namespace SlowAndReverb
{
    public class Light : Component
    {
        private float _radius;

        public Light(Color color, float radius)
        {
            Color = color;
            Radius = radius;
        }

        public Light()
        {

        }

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

        public Rectangle Bounds
        {
            get
            {
                Vector2 roundedPosition = Position.Floor();
                Vector2 radiusVector = new Vector2(Radius);

                return new Rectangle(roundedPosition - radiusVector, roundedPosition + radiusVector);
            }
        }
    }
}
