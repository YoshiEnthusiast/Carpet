namespace Carpet
{
    public abstract class ShapeParticleEmitter : ParticleEmitter
    {
        public ShapeParticleEmitter(ParticleBehavior behavior) : base(behavior)
        {

        }

        public ParticleShapeBehavior ShapeBehavior { get; set; }

        protected override Vector2 GeneratePosition()
        {
            if (ShapeBehavior == ParticleShapeBehavior.Fill)
                return GeneratePositionInsideShape();

            return GeneratePositionAroundBorder();
        }

        protected abstract Vector2 GeneratePositionInsideShape();
        protected abstract Vector2 GeneratePositionAroundBorder();
    }
}
