using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SlowAndReverb
{
    public abstract class Component
    {
        // Add null checks??????
        public Scene Scene => Entity.Scene;
        public Vector2 Position => Entity.Position;

        public Entity Entity { get; set; }

        public void DoUpdate(float deltaTime)
        {
            // Remove this
            if (Entity is null)
                return;

            Update(deltaTime);
        }

        public void DoDraw()
        {
            if (Entity is null)
                return;

            Draw();
        }

        public virtual void Update(float deltaTime)
        {

        }

        public virtual void Draw()
        {

        }

        public virtual void OnAdded()
        {

        }

        public virtual void OnRemoved(Entity from)
        {

        }
    }
}
