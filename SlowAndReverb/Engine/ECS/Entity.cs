using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Runtime.InteropServices;

namespace SlowAndReverb
{
    public abstract class Entity
    {
        private readonly ComponentCollection _components;

        public Entity(float x, float y)
        {
            Position = new Vector2(x, y);

            _components = new ComponentCollection(this);
        }

        public float X
        {
            get
            {
                return Position.X;
            }

            set
            {
                Position = new Vector2(value, Position.Y);
            }
        }

        public float Y
        {
            get
            {
                return Position.Y;
            }

            set
            {
                Position = new Vector2(Position.X, value);
            }
        }

        public float Width
        {
            get
            {
                return Size.X;
            }

            set
            {
                Size = new Vector2(value, Size.Y);
            }
        }

        public float Height
        {
            get
            {
                return Size.Y;
            }

            set
            {
                Size = new Vector2(Size.X, value);
            }
        }

        public Scene Scene { get; private set; }

        public bool Awake { get; set; } = true;
        public bool Visible { get; set; } = true;
        public bool NeverMoves { get; set; }

        public Vector2 Position { get; set; }
        public Vector2 Size { get; set; }
        public Vector2 Center { get; set; }

        public IEnumerable<Component> Components => _components;

        public float HalfWidth => Width / 2f;
        public float HalfHeight => Height / 2f;

        public Vector2 OffsetPosition => Position + Center;
        public Vector2 HalfSize => new Vector2(HalfWidth, HalfHeight);

        public float Left => OffsetPosition.X - HalfWidth;
        public float Top => OffsetPosition.Y - HalfHeight;
        public float Right => OffsetPosition.X + HalfWidth;
        public float Bottom => OffsetPosition.Y + HalfHeight;

        public Vector2 TopLeft => new Vector2(Left, Top);
        public Vector2 TopRight => new Vector2(Right, Top);
        public Vector2 BottomLeft => new Vector2(Left, Bottom);
        public Vector2 BottomRight => new Vector2(Right, Bottom);

        public Rectangle Rectangle => new Rectangle(TopLeft, BottomRight);

        public void DoUpdate(float deltaTime)
        {
            _components.Update();

            if (!Awake)
                return;

            foreach (Component component in _components)
                component.DoUpdate(deltaTime);

            Update(deltaTime);
        }

        public void DoDraw()
        {
            if (!Visible)
                return;

            foreach (Component component in _components)
                component.DoDraw();

            Draw();
        }

        public bool CollidesWith(Rectangle rectangle)
        {
            return Rectangle.Intersects(rectangle);
        }

        public bool CollidedsWith(Entity entity)
        {
            return CollidesWith(entity.Rectangle);
        }

        public T Add<T>(T component) where T : Component
        {
            _components.Add(component);

            return component;
        }

        public void Remove<T>(T component) where T : Component
        {
            _components.Remove(component);
        }

        public T Get<T>() where T : Component
        {
            foreach (Component component in _components)
                if (component is T result)
                    return result;

            return null;
        }

        public void Added(Scene to)
        {
            Scene = to;

            OnAdded();
        }

        public void Removed()
        {
            Scene from = Scene;
            Scene = null;

            OnRemoved(from);
        }

        public Vector2 Rotate(Vector2 position, float angle)
        {
            return position.Rotate(Position, angle);
        }

        public Vector2 Rotate(float length, float angle)
        {
            Vector2 position = Position + new Vector2(length, 0f);

            return Rotate(position, angle);
        }

        protected virtual void OnAdded()
        {

        }

        protected virtual void OnRemoved(Scene from)
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
