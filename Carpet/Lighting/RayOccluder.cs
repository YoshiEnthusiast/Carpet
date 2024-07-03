namespace Carpet
{
    public class RayOccluder : Component
    {
        private readonly SolidColorMaterial _material = new();

        public RayOccluder()
        {

        }

        public OcclusionMode Mode { get; set; }

        public Sprite Sprite { get; set; }
        public int CircleRadius { get; set; } = 20;

        public void DrawOcclusion()
        {
            Color color = Color.Black;

            switch (Mode)
            {
                case OcclusionMode.EntityRectangle:
                    Graphics.FillRectangle(EntityRectangle, _material, color, 1f);
                    break;

                case OcclusionMode.Circle:
                    Graphics.FillCircle(Position, color, CircleRadius, 1f);
                    break;

                case OcclusionMode.SpriteComponent:
                    Sprite sprite = Entity.Get<Sprite>();

                    if (sprite is not null)
                        sprite.Draw(_material, sprite.Position, sprite.Scale, sprite.Origin, color,
                                sprite.Angle, sprite.HorizontalEffect, sprite.VerticalEffect, 1f);

                    break;

                case OcclusionMode.CustomSprite:
                    if (Sprite is not null)
                    {
                        Sprite.Material = _material;
                        Sprite.Color = Color.Black;
                        Sprite.Depth = 1f;
                    }

                    break;
            }
        }
    }
}
