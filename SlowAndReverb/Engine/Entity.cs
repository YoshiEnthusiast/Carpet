using OpenTK.Windowing.Desktop;
using System;
using System.Collections.Generic;

namespace SlowAndReverb
{
    public abstract class Entity
    {
        // Component collection?????
        private readonly List<Component> _components = new List<Component>();

        public Entity(float x, float y)
        {
            Position = new Vector2(x, y);
        }

        public Scene Scene { get; private set; }
        public IEnumerable<Component> Components => _components;

        public Vector2 HalfSize => Size / 2f;
        public Rectangle Rectangle => new Rectangle(Position - HalfSize, Position + HalfSize);

        public Vector2 TopLeft => new Vector2(Left, Top);
        public Vector2 TopRight => new Vector2(Right, Top);
        public Vector2 BottomLeft => new Vector2(Left, Bottom);
        public Vector2 BottomRight => new Vector2(Right, Bottom);

        public float Left => X - HalfSize.X;
        public float Top => Y - HalfSize.Y;
        public float Right => X + HalfSize.X;    
        public float Bottom => Y + HalfSize.Y;

        public Vector2 Position { get; set; }
        public Vector2 Size { get; set; }
        public bool Awake { get; set; } = true;
        public bool Visible { get; set; } = true;
        public bool NeverMoves { get; protected init; }

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

        public void DoUpdate(float deltaTime)
        {
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

        public virtual void Update(float deltaTime)
        {
            
        }

        public virtual void Draw()
        {

        }

        public virtual void OnAdded(Scene to)
        {
            Scene = to;
        }

        public virtual void OnRemoved()
        {
            Scene = null;
        }

        public virtual void OnInitialize()
        {

        }

        public T GetComponent<T>() where T : Component
        {
            foreach (Component component in _components)
                if (component is T result)
                    return result;

            return default(T);
        }

        public IEnumerable<T> GetComponents<T>() where T : Component
        {
            foreach (Component component in _components)
                if (component is T result)
                    yield return result;
        }

        public T Add<T>(T component) where T : Component
        {
            if (_components.Contains(component))
                return null;

            Entity entity = component.Entity;

            if (entity is not null)
                entity.Remove(component);

            _components.Add(component);
            Scene?.OnComponentAdded(this, component);
            component.Entity = this;

            component.OnAdded();

            return component;
        }

        public void Remove(Component component)
        {
            if (!_components.Contains(component))
                return;

            _components.Remove(component);
            Scene?.OnComponentRemoved(this, component);
            component.Entity = null;

            component.OnRemoved(this);
        }

        public void DrawCollision(float depth)
        {
            Graphics.DrawRectangle(Rectangle, Color.Blue, depth);
        }

        public void Enable()
        {
            Awake = true;
            Visible = true;
        }

        public void Disable()
        {
            Awake = false;
            Visible = false;
        }

        public bool ColliderWith(Rectangle rectangle)
        {
            return Rectangle.Intersects(rectangle);
        }
    }
}
