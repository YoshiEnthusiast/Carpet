namespace Carpet.Examples.RayCasting
{
    public class WhiteLight : Entity
    {
        public WhiteLight(float x, float y) : base(x, y)
        {
            Add(new Light()
            {
                Color = Color.White,
                Radius = 80f,
                Volume = 0.4f,
                Angle = Maths.HalfPI,
                Rotation = Maths.HalfPI,
                FalloffAngle = 0.4f
            });
        }
    }
}

