using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Carpet.Platforming
{
    public class TestEntity : Entity
    {
        private readonly Light[] _lights;

        public TestEntity(float x, float y) : base(x, y)
        {
            _lights = new Light[]
            {
                Add(new Light()
                {
                    Color = Color.Red
                }),

                Add(new Light()
                {
                    Color = new Color(255, 115, 3)
                }),

                Add(new Light()
                {
                    Color = new Color(255, 211, 3)
                }),

                Add(new Light()
                {
                    Color = new Color(73, 252, 3)
                }),

                Add(new Light()
                {
                    Color = new Color(3, 7, 252)
                }),

                Add(new Light()
                {
                    Color = new Color(251, 0, 255)
                }),
            };

            int length = _lights.Length;

            for (int i = 0; i < length; i++)
            {
                Light light = _lights[i];

                light.Rotation = Maths.TwoPI / length * i;

                light.Radius = 60f;
                light.Angle = Maths.PI / 3f;
                light.FalloffAngle = 0.4f;
                light.StartDistance = 0.2f;
                light.StartFade = 0.1f;
                light.Volume = 0.8f;
            }
        }

        protected override void Update(float deltaTime)
        {
            Position = Game.ForegroundLayer.MousePosition;

            foreach (Light light in _lights)
                light.Rotation += 0.01f;
        }
    }
}
