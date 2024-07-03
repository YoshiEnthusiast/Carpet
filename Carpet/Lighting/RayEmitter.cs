namespace Carpet
{
    public class RayEmitter : Component
    {
        private readonly SolidColorMaterial _intensityMaterial = new();

        public RayEmitter()
        {

        }

        public OcclusionMode Mode { get; set; }

        public Color Color { get; set; } = Color.White;
        public Sprite Sprite { get; set; }
        public int CircleRadius { get; set; } = 20;
        public float Intensity { get; set; } = 1f;

        public void DrawEmission()
        {
            switch (Mode)
            {
                case OcclusionMode.EntityRectangle:
                    Graphics.FillRectangle(EntityRectangle, Color, 0f);
                    break;

                case OcclusionMode.Circle:
                    Graphics.FillCircle(Position, Color, CircleRadius, 0f);
                    break;

                case OcclusionMode.SpriteComponent:
                    Sprite sprite = Entity.Get<Sprite>();

                    if (sprite is not null)
                        sprite.Draw(sprite.Position);

                    break;

                case OcclusionMode.CustomSprite:
                    if (Sprite is not null)
                        Sprite.Draw(Sprite.Position);

                    break;
            }
        }

        public void DrawIntensity()
        {
            var color = new Color(Intensity, 0f, 0f, 1f);

            switch (Mode)
            {
                case OcclusionMode.EntityRectangle:
                    Graphics.FillRectangle(EntityRectangle, _intensityMaterial, color, 0f);
                    break;

                case OcclusionMode.Circle:
                    Graphics.FillCircle(Position, color, CircleRadius, 0f);
                    break;

                case OcclusionMode.SpriteComponent:
                    Sprite sprite = Entity.Get<Sprite>();

                    if (sprite is not null)
                        sprite.Draw(sprite.Material, sprite.Position, sprite.Scale, sprite.Origin, color,
                                sprite.Angle, sprite.HorizontalEffect, sprite.VerticalEffect, 0f);

                    break;

                case OcclusionMode.CustomSprite:
                    Sprite.Draw(_intensityMaterial, Sprite.Position, Sprite.Scale, Sprite.Origin, color,
                            Sprite.Angle, Sprite.HorizontalEffect, Sprite.VerticalEffect, 0f);

                    break;
            }
        }
    }
}
