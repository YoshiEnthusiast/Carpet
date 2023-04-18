using System.Security.Cryptography;

namespace SlowAndReverb
{
    public abstract class Component
    {
        public Vector2 Position
        {
            get
            {
                if (OverridenPosition is not null)
                    return OverridenPosition.Value;

                return EntityPosition + PositionOffset;
            }
        }

        public Entity Entity { get; private set; }

        public Vector2? OverridenPosition { get; set; }
        public Vector2 PositionOffset { get; set; }

        public bool Awake { get; set; } = true;
        public bool Visible { get; set; } = true;

        public Scene Scene => Entity?.Scene;
        public Vector2 EntityPosition => Entity.Position;
        public Rectangle EntityRectangle => Entity.Rectangle;

        public void DoUpdate(float deltaTime)
        {
            if (Awake)
                Update(deltaTime);
        }

        public void DoDraw()
        {
            if (Visible)
                Draw();
        }

        public void Added(Entity to)
        {
            Entity = to;

            OnAdded();
        }

        public void Removed()
        {
            Entity from = Entity;
            Entity = null;

            OnRemoved(from);
        }

        protected virtual void OnAdded()
        {

        }

        protected virtual void OnRemoved(Entity from)
        {

        }

        protected virtual void Update(float deltaTime)
        {

        }

        protected virtual void Draw()
        {

        }
    }
}
