using OpenTK.Windowing.GraphicsLibraryFramework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Reflection.Metadata;
using System.Reflection.Metadata.Ecma335;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Carpet
{
    public static class ImGui
    {
        private const float DepthStep = 0.01f;

        private static readonly Stack<Layout> s_layouts = [];

        private static readonly List<Menu> s_menus = [];
        private static Menu s_currentMenu;

        private static string s_activeId;

        private static bool s_began;

        private static Vector2 s_lastMousePosition;
        private static Vector2 s_drag;
        private static bool s_dragging;

        public static Font Font { get; set; } = new Font("testFont");

        public static float Margin { get; set; } = 5f;
        public static Vector2 TextPadding { get; set; } = new Vector2(2f);

        private static Vector2 MousePosition => Graphics.CurrentLayer.MousePosition;
        private static bool IsMousePressed => Input.IsMousePressed(MouseButton.Left);
        private static bool IsMouseDown => Input.IsMouseDown(MouseButton.Left);
        private static bool IsMouseReleased => Input.IsMouseReleased(MouseButton.Left);

        public static void Begin()
        {
            if (s_began)
                throw new InvalidOperationException($"{nameof(End)} must be called before calling {nameof(Begin)} again");

            s_began = true;
        }

        public static void End()
        {
            if (!s_began)
                throw new InvalidOperationException($"{nameof(End)} can only be called after {nameof(Begin)} is called");

            if (s_currentMenu is not null)
                EndMenu();

            s_began = false;
        }

        public static void BeginMenu(string text, LayoutType layoutType)
        {
            CheckBegin(nameof(BeginMenu));

            if (s_currentMenu is not null)
                throw new InvalidOperationException("Active menu has to be ended before beginning a new one");

            if (IsMousePressed)
            {
                s_lastMousePosition = MousePosition;
                s_dragging = true;
            }

            if (IsMouseDown)
            {
                s_drag = MousePosition - s_lastMousePosition;
                s_lastMousePosition = MousePosition;
            }
            else
            {
                s_dragging = false;
            }

            s_currentMenu = GetMenu(text);

            BeginLayout(layoutType);
        }

        public static void BeginMenu(string text)
        {
            BeginMenu(text, LayoutType.Vertical);
        }

        public static void EndMenu()
        {
            CheckBegin(nameof(EndMenu));

            if (s_currentMenu is null)
                throw new InvalidOperationException("No active menu");

            string menuName = s_currentMenu.Name;
            Vector2 position = s_currentMenu.Position;

            while (s_layouts.Count > 1)
                EndLayout();

            Layout baseLayout = s_layouts.Pop();
            Vector2 size = baseLayout.Size;
            var rectangle = new Rectangle(position, position + size);

            if (IsActive(menuName))
            {
                if (IsMouseReleased)
                    s_activeId = null;
                else if (s_dragging)
                    s_currentMenu.Position += s_drag;
            }
            else if (s_activeId is null)
            {
                if (ContainsMouse(rectangle) && IsMousePressed)
                    s_activeId = menuName;
            }

            Graphics.DrawRectangle(rectangle, Color.White * 0.4f, 0f);

            s_layouts.Clear();
            s_currentMenu = null;
        }

        public static void BeginLayout(LayoutType type)
        {
            CheckBegin(nameof(BeginLayout));

            Vector2 position = Vector2.Zero;

            if (s_layouts.TryPeek(out Layout previousLayout)) // TODO: TryGetNextPosition
                position = GetNextPositionLocal(previousLayout);

            var layout = new Layout()
            {
                Type = type,
                Position = position
            };

            s_layouts.Push(layout);
        }

        public static void EndLayout()
        {
            CheckBegin(nameof(EndLayout));

            if (s_layouts.Count < 1)
                throw new InvalidOperationException("Cannot end a layout: no active layout");

            Layout activeLayout = s_layouts.Pop();

            if (s_layouts.TryPeek(out Layout layout)) // TODO: TryExpandLayout
                ExpandLayout(activeLayout.Size);
        }

        public static void Label(string text)
        {
            CheckBegin(nameof(Label));
            Vector2 position = GetNextPosition();

            Font.Draw(text, position, Color.White, 0f);

            Vector2 size = Font.Measure(text);
            ExpandLayout(size);
        }

        public static bool Button(string text, Color color)
        {
            CheckBegin(nameof(Button));
            Vector2 position = GetNextPosition();

            Vector2 textSize = Font.Measure(text);

            Vector2 size = textSize + TextPadding * 2f;
            var rectangle = new Rectangle(position, position + size);

            Graphics.DrawRectangle(rectangle, color, 1f);
            Font.Draw(text, position + TextPadding, Color.White, 1f);

            bool click = false;
            bool hovered = ContainsMouse(rectangle);

            if (IsActive(text))
            {
                if (IsMouseReleased)
                {
                    if (hovered)
                        click = true;

                    s_activeId = null;
                }
            }
            else if (s_activeId is null)
            {
                if (hovered && IsMousePressed) // TODO: Do I need to check if this is hot?
                    s_activeId = text;
            }

            ExpandLayout(size);

            return click;
        }

        private static Menu GetMenu(string name)
        {
            foreach (Menu menu in s_menus)
                if (menu.Name == name)
                    return menu;

            var newMenu = new Menu(name);

            foreach (Menu menu in s_menus)
                menu.Depth++;

            s_menus.Add(newMenu);

            return newMenu;
        }

        private static void PushToTop(Menu menu)
        {
            foreach (Menu other in s_menus)
                if (other.Depth < menu.Depth)
                    other.Depth++;

            menu.Depth = 0f;
        }

        private static bool IsActive(string id)
        {
            return id == s_activeId;
        }

        private static Vector2 GetNextPosition(Layout layout)
        {
            Vector2 localPosition = GetNextPositionLocal(layout);

            return s_currentMenu.Position + localPosition;
        }

        private static Vector2 GetNextPositionLocal(Layout layout)
        {
            Vector2 position = layout.Position;
            Vector2 size = layout.Size;

            float x = 0f;
            float y = 0f;

            switch (layout.Type)
            {
                case LayoutType.Horizontal:
                    x = position.X + size.X;
                    y = position.Y;

                    break;

                case LayoutType.Vertical:
                    x = position.X;
                    y = position.Y + size.Y;

                    break;
            }

            return new Vector2(x, y);
        }

        private static Vector2 GetNextPosition()
        {
            if (s_layouts.TryPeek(out Layout layout))
                return GetNextPosition(layout);

            throw GetNoLayoutException();
        }

        private static void ExpandLayout(Vector2 by)
        {
            if (s_layouts.TryPop(out Layout layout))
            {
                Vector2 size = layout.Size;

                float width = 0f;
                float height = 0f;

                switch (layout.Type)
                {
                    case LayoutType.Horizontal:
                        width = size.X + by.X + Margin;
                        height = Maths.Max(size.Y, by.Y);

                        break;

                    case LayoutType.Vertical:
                        width = Maths.Max(size.X, by.X);
                        height = size.Y + by.Y + Margin;

                        break;
                }

                layout.Size = new Vector2(width, height);

                s_layouts.Push(layout);

                return;
            }

            throw GetNoLayoutException();
        }

        private static bool ContainsMouse(Rectangle rectangle)
        {
            return rectangle.Contains(MousePosition);
        }

        private static InvalidOperationException GetNoLayoutException()
        {
            return new InvalidOperationException("No active layout");
        }

        private static void CheckBegin(string methodName)
        {
            if (!s_began)
                throw new InvalidOperationException($"{methodName} can only be called after Begin is called");
        }

        public enum LayoutType
        {
            Horizontal,
            
            Vertical
        }

        private sealed class Menu
        {
            public Menu(string name)
            {
                Name = name;
            }

            public Vector2 Position { get; set; }
            public float Depth { get; set; }
            public string Name { get; private init; }
        }

        private struct Layout
        {
            public LayoutType Type { get; init; }

            public Vector2 Position { get; init; }
            public Vector2 Size { get; set; }
        }
    }
}
