using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Carpet
{
    public class SpinningBoxes : Entity
    {
        private readonly Sprite _sprite;

        public SpinningBoxes(float x, float y) : base(x, y)
        {
            _sprite = Add(new Sprite("spinningBoxes"));
            _sprite.SetCenterOrigin();

            Add(new Light()
            {
                Radius = 50f,
                Color = Color.LimeGreen,
                Volume = 0.6f
            });

            Add(new LightOccluder(OcclusionMode.SpriteComponent));
        }

        protected override void Update(float deltaTime)
        {
            _sprite.Angle += 0.015f;
        }
    }
}
