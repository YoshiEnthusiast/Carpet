using System;
using System.Collections;
using System.Collections.Generic;

namespace SlowAndReverb
{
    public abstract class UIElement
    {
        private float _depth;

        public UIElement()
        {
            Children = new UIElementCollection(this);
        }

        public float Depth
        {
            get
            {
                if (Parent is null)
                    return _depth;

                return Parent.Depth;
            }

            set
            {
                _depth = value;
            }
        }

        public Vector2 ScreenPosition
        {
            get
            {
                Vector2 position = Position;

                if (Parent is not null)
                    position += Parent.ScreenPosition;

                return position;
            }
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

        public IEnumerable<UIElement> ChildrenChain
        {
            get
            {
                foreach (UIElement child in Children)
                {
                    yield return child;

                    foreach (UIElement grandChild in child.ChildrenChain)
                        yield return grandChild;
                }
            }
        }

        public IEnumerable<UIElement> ParentsChain
        {
            get
            {
                UIElement parent = Parent;

                while (parent is not null)
                {
                    yield return parent;

                    parent = parent.Parent;
                }
            }
        }

        public Vector2 Position { get; set; }
        public Vector2 Size { get; set; }

        public Color BackgroundColor { get; set; }
        public Color BorderColor { get; set; }
        public int BorderWidth { get; set; } = 1;

        public UIElementCollection Children { get; private init; } 
        public UIElement Parent { get; private set; }

        public Font Font { get; set; } = new Font("testFont");
        public string Text { get; set; } = string.Empty;

        public Rectangle Rectangle => new Rectangle(Position, Position + Size);
        public Rectangle ScreenRectangle => new Rectangle(ScreenPosition, ScreenPosition + Size);

        public Vector2 HalfSize => Size / 2f;
        public Vector2 Center => Position + HalfSize;

        public float Width => Size.X;
        public float Height => Size.Y;
        public float HalfWidth => HalfSize.X;
        public float HalfHeight => HalfSize.Y;

        public float ScreenX => ScreenPosition.X;
        public float ScreenY => ScreenPosition.Y;

        public void DoUpdate(float deltaTime)
        {
            Children.Update();

            Update(deltaTime);

            foreach (UIElement child in Children) 
                child.DoUpdate(deltaTime);
        }

        public void DoDraw()
        {
            Draw();

            foreach (UIElement child in Children)
                child.DoDraw();
        }

        public void Added(UIElement to)
        {
            Parent = to;

            OnAdded();
        }

        public void Removed()
        {
            UIElement from = Parent;
            Parent = null;

            OnRemoved(from);
        }

        protected virtual void Update(float deltaTime)
        {

        }

        protected virtual void Draw()
        {
            FillRectangle(ScreenRectangle, BackgroundColor);
            DrawRectangle(ScreenRectangle, BorderColor, BorderWidth);
        }

        protected virtual void OnAdded()
        {

        }

        protected virtual void OnRemoved(UIElement from)
        {
            
        }

        protected void DrawRectangle(Rectangle rectangle, Color color, int lineWidth = 1, bool centered = false)
        {
            Graphics.DrawRectangle(rectangle, color, Depth, lineWidth, centered);
        }

        protected void FillRectangle(Rectangle rectangle, Color color)
        {
            Graphics.FillRectangle(rectangle, color, Depth);
        }
    }
}
