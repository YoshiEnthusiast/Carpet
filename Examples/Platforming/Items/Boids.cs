using System;
using System.Runtime.InteropServices;
using ComputeItem = Carpet.Std430LayoutItem;

namespace Carpet.Examples.Platforming
{
    public class Boids : Entity
    {
        private const float BorderOffset = 2f;

        private readonly Boid[] _boids = new Boid[BoidsComputer.MaxBoids];
        private readonly Vector2[] _acceleration = new Vector2[BoidsComputer.MaxBoids];
        private readonly Sprite _sprite = new("boid");

        private BoidsComputer _computer;
        private int _bufferHandle;

        private int _boidsCount = 200;

        public Boids(float x, float y) : base(x, y)
        {

        }

        public int BoidsCount
        {
            get
            {
                return _boidsCount;
            }

            set
            {
                int count = Maths.Min(value, BoidsComputer.MaxBoids);

                _boidsCount = count;
            }
        }

        public float MaxSpeed { get; set; } = 2f;
        public float PerceptionRadius { get; set; } = 12f;

        public float AlignmentWeight { get; set; } = 0.15f; 
        public float CohesionWeight { get; set; } = 0.07f; 
        public float SeparationWeight { get; set; } = 0.05f; 

        protected override void Update(float deltaTime)
        {
            _computer.BindBuffer(_bufferHandle);
            _computer.SetItem(0, _boids);

            _computer.PerceptionRadius = PerceptionRadius;
            _computer.MaxSpeed = MaxSpeed;

            _computer.AlignmentWeight = AlignmentWeight;
            _computer.CohesionWeight = CohesionWeight;
            _computer.SeparationWeight = SeparationWeight;

            _computer.Compute(BoidsCount, 1, 1);

            _computer.GetItem(1, _acceleration);

            for (int i = 0; i < BoidsCount; i++)
            {
                Vector2 acceleration = _acceleration[i];

                Vector2 velocity = _boids[i].Velocity + acceleration;
                velocity = velocity.Clamp(-MaxSpeed, MaxSpeed);
                _boids[i].Velocity = velocity;

                Vector2 position = _boids[i].Position + _boids[i].Velocity;
                float x = position.X;
                float y = position.Y;

                if (x > Width)
                    x = BorderOffset;
                else if (x < 0f)
                    x = Width - BorderOffset;

                if (y > Height)
                    y = BorderOffset;
                else if (y < 0f)
                    y = Height - BorderOffset;

                _boids[i].Position = new Vector2(x, y);
            }
        }

        protected override void Draw()
        {
            for (int i = 0; i < _boidsCount; i++)
            {
                Boid boid = _boids[i];

                Vector2 velocity = boid.Velocity;
                Vector2 position = TopLeft + boid.Position;

                float angle = Maths.Atan2(velocity.Y, velocity.X);
                _sprite.Angle = angle;

                float lerpAmount = 1f / (i % 4 + 1);
                Color color = Color.White.Lerp(Color.Blue, lerpAmount);
                _sprite.Color = color;

                _sprite.Draw(position);
            }

            Graphics.DrawRectangle(Rectangle, Color.Red, 0f);
        }

        protected override void OnAdded()
        {
            int maxBoids = BoidsComputer.MaxBoids;

            ReadOnlySpan<ComputeItem> items = stackalloc ComputeItem[]
            {
                ComputeItem.StructArray<Boid, Vector2>(maxBoids),
                ComputeItem.Array<Vector2>(maxBoids)
            };

            _computer = new BoidsComputer();
            _computer.BoidsCount = BoidsCount;

            _bufferHandle = _computer.PushBuffer(items);

            for (int i = 0; i < BoidsCount; i++)
            {
                Vector2 position = Random.NextVector2(Vector2.Zero, Size);
                Vector2 velocity = Random.NextVector2(new Vector2(-MaxSpeed), new Vector2(MaxSpeed));

                _boids[i].Position = position;
                _boids[i].Velocity = velocity;
            }
        }

        protected override void OnRemoved(Scene from)
        {
            _computer.Free();
        }

        [StructLayout(LayoutKind.Sequential)]
        private record struct Boid(Vector2 Position, Vector2 Velocity);
    }

    public sealed class BoidsComputer : Computer
    {
        public const int MaxBoids = 1000;

        public BoidsComputer()
        {
            ShaderProgram = Content.GetComputeShaderProgram("boids");
        }

        [Uniform("u_BoidsCount")] public int BoidsCount { get; set; } = 20;

        [Uniform("u_PerceptionRadius")] public float PerceptionRadius { get; set; } = 10f;
        [Uniform("u_MaxSpeed")] public float MaxSpeed { get; set; } = 2f;

        [Uniform("u_AlignmentWeight")] public float AlignmentWeight { get; set; } = 2f;
        [Uniform("u_CohesionWeight")] public float CohesionWeight { get; set; } = 0.5f;
        [Uniform("u_SeparationWeight")] public float SeparationWeight { get; set; } = 0.3f;
    }
}
