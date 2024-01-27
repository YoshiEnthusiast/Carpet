using System;
using System.Linq;

namespace Carpet
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
            // if (Input.IsPressed(Key.I))
            //     Scene.GetEntityOfType<Player>().Position = new Vector2(100f, 80f);

            // //Console.WriteLine(Input.XAxis.GetValue());

            // Vector2 mousePosition = Demo.ForegroundLayer.MousePosition;

            // if (Input.IsMousePressed(MouseButton.Left))
            // {
            //     Scene.Add(new GrapplableCoin(mousePosition.X, mousePosition.Y));

            //     //float x = Maths.Floor(mousePosition.X / 8f) * 8f + 4f;
            //     //float y = Maths.Floor(mousePosition.Y / 8f) * 8f + 4f;

            //     //var v = new Vector2(x, y);

            //     //if (Scene.CheckPosition<AutoTile>(v) is null)
            //     //{
            //     //    Scene.Add(new DefaultFakeBlock(x, y)
            //     //    {

            //     //    });

            //     //    foreach (AutoTile b in Scene.CheckCircleAll<AutoTile>(v, 16))
            //     //    {
            //     //        b.NeedsRefresh = true;
            //     //    }
            //     //}
            // }
            // else if (Input.IsMousePressed(MouseButton.Right))
            // {
            //     foreach (BlockGroup group in Scene.Entities.OfType<BlockGroup>())
            //         Scene.Remove(group);

            //     foreach (AutoTile block in Scene.Entities.OfType<AutoTile>())
            //         block.NeedsRefresh = true;

            //     Scene.GetSystem<BlockGroupsSystem>().Initialize();
            // }
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
