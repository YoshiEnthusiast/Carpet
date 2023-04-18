namespace SlowAndReverb
{
    public class ParticleBehaviour
    {
        public Entity Follow { get; set; }

        public Sprite Sprite { get; set; }
        public float Depth { get; set; } = Depths.Particles;

        public Vector2 Velocity { get; set; } = Vector2.One;
        public Vector2 VelocityVariation { get; set; } = Vector2.Zero;

        public Vector2 Scale { get; set; } = Vector2.One;
        public Vector2 ScaleVariation { get; set; } = Vector2.Zero;
        public Vector2 DestinationScale { get; set; } = Vector2.One;

        public float LifeDuration { get; set; } = 10f;
        public float LifeDurationVariation { get; set; }

        public Color StartingColor { get; set; } = Color.White;
        public Color DestinationColor { get; set; }

        public virtual ParticleData Create(Vector2 position, Vector2 sourcePosition)
        {
            Vector2 velocity = Velocity + Random.NextVector2(-VelocityVariation, VelocityVariation);
            Vector2 scale = Scale + Random.NextVector2(-ScaleVariation, ScaleVariation);

            float lifeDuration = LifeDuration + Random.NextFloat(-LifeDurationVariation, LifeDurationVariation);

            return new ParticleData(this)
            {
                Position = position,
                Velocity = velocity,
                Scale = scale,
                LifeDuration = lifeDuration,
                Follow = Follow
            }; 
        }

        public virtual void Update(ref Particle particle, float deltaTime)
        {
            particle.Life += deltaTime;
            float life = particle.Life;

            if (life >= particle.LifeDuration)
            {
                particle.Active = false;

                return;
            }

            float lifeNormalized = particle.Life / particle.LifeDuration;

            UpdatePosition(ref particle, lifeNormalized, deltaTime);
            UpdateScale(ref particle, lifeNormalized, deltaTime);
            UpdateColor(ref particle, lifeNormalized, deltaTime);

            if (Sprite is not null)
            {
                Sprite.DoUpdate(deltaTime);
                Sprite.Scale = particle.Scale;
            }
        }

        public virtual ParticleBehaviour Copy()
        {
            return new ParticleBehaviour()
            {
                Follow = Follow,
                Sprite = Sprite,
                Depth = Depth,
                Velocity = Velocity,
                VelocityVariation = VelocityVariation,
                Scale = Scale,
                ScaleVariation = ScaleVariation,
                DestinationScale = DestinationScale,
                LifeDuration = LifeDuration,
                LifeDurationVariation = LifeDurationVariation,
                StartingColor = StartingColor,
                DestinationColor = DestinationColor
            };
        }

        protected virtual void UpdatePosition(ref Particle particle, float lifeNormalized, float deltaTime)
        {
            particle.Position += particle.Velocity;
        }

        protected virtual void UpdateScale(ref Particle particle, float lifeNormalized, float deltaTime)
        {
            particle.Scale = Vector2.Lerp(particle.InitialScale, DestinationScale, lifeNormalized);
        }

        protected virtual void UpdateColor(ref Particle particle, float lifeNormalized, float deltaTime)
        {
            particle.Color = Color.Lerp(StartingColor, DestinationColor, lifeNormalized);
        }

        public virtual void Draw(ref Particle particle)
        {
            Vector2 position = particle.Position;

            if (Sprite is null)
            {
                Graphics.FillRectangle(new Rectangle(position, position + particle.Scale), particle.Color, Depth);

                return;
            }

            Sprite.Draw(position);
        }
    }
}
