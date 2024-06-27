namespace Carpet
{
    public class RectangleParticleEmitter : ShapeParticleEmitter
    {
        private readonly float _width;
        private readonly float _height;

        public RectangleParticleEmitter(ParticleBehavior behavior, float width, float height) : base(behavior)
        {
            _width = width;
            _height = height;
        }

        private Vector2 HalfSize => new Vector2(_width, _height) / 2f;

        protected override Vector2 GeneratePositionInsideShape()
        {
            float xOffset = Random.NextFloat(_width);
            float yOffset = Random.NextFloat(_height);

            return Position + new Vector2(xOffset, yOffset) - HalfSize;
        }

        protected override Vector2 GeneratePositionAroundBorder()
        {
            float x = Position.X;
            float y = Position.Y;

            if (Random.NextBool())
            {
                x += _width * Random.NextBinary();
                y += _height * Random.NextFloat();
            }
            else
            {
                x += _width * Random.NextFloat();
                y += _height * Random.NextBinary();
            }

            return new Vector2(x, y) - HalfSize;
        }
    }
}
