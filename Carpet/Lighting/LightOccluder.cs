﻿namespace Carpet
{
    public class LightOccluder : Component
    {
        public OcclusionMode Mode { get; set; }

        public Sprite Sprite { get; set; }
        public int CircleRadius { get; set; }

        public void DrawOcclusion()
        {
            switch (Mode)
            {
                case OcclusionMode.EntityRectangle:
                    Graphics.FillRectangle(EntityRectangle, Color.White, 0f);
                    break;

                case OcclusionMode.Circle:
                    Graphics.FillCircle(Position, Color.White, CircleRadius, 0f);
                    break;

                case OcclusionMode.SpriteComponent:
                    Sprite sprite = Entity.Get<Sprite>();

                    if (sprite is not null)
                        sprite.Draw(Position);

                    break;

                case OcclusionMode.CustomSprite:
                    if (Sprite is not null)
                        Sprite.Draw(Position);

                    break;
            }
        }
    }
}
