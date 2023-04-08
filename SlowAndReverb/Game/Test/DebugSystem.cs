using System;
using System.Linq;

namespace SlowAndReverb
{
    public class DebugSystem : System
    {
        public DebugSystem(Scene scene) : base(scene)
        {

        }

        public override void Initialize()
        {
            
        }

        public override void Update(float deltaTime)
        {
            Vector2 mousePosition = Layers.Foreground.MousePosition;

            if (Input.IsMouseDown(MouseButton.Left))
            {
                float x = Maths.Floor(mousePosition.X / 8f) * 8f + 4f;
                float y = Maths.Floor(mousePosition.Y / 8f) * 8f + 4f;

                var v = new Vector2(x, y);

                if (Scene.CheckPoint<Block>(v) is null)
                {
                    Scene.Add(new Block("tileset", x, y)
                    {
                        Fake = true
                    });

                    foreach (Block b in Scene.CheckCircleAll<Block>(v, 16))
                        b.NeedsRefresh = true;
                }
            }
            else if (Input.IsMousePressed(MouseButton.Right))
            {
                foreach (BlockGroup group in Scene.Entities.OfType<BlockGroup>())
                    Scene.Remove(group);

                foreach (Block block in Scene.Entities.OfType<Block>())
                    block.NeedsRefresh = true;

                Scene.GetSystem<BlockGroupsSystem>().Initialize();
            }
        }

        public override void OnBeforeDraw()
        {
            
        }

        public override void Draw()
        {
            
        }

        public override void OnLateDraw()
        {
            
        }
    }
}
