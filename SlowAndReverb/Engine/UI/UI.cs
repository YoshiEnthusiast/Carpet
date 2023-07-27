using OpenTK.Windowing.GraphicsLibraryFramework;
using System;
using System.Collections.Generic;
using System.Threading;

namespace SlowAndReverb
{
    public static class UI
    {
        private static readonly DynamicCollection<UIRoot> s_roots = new DynamicCollection<UIRoot>();
        private static Sprite s_cursorSprite;

        public static CursorMode CursorMode { get; set; }

        public static Vector2 MousePosition => Layers.UI.MousePosition;

        internal static void Initialize()
        {
            s_cursorSprite = new Sprite("cursor")
            {
                Origin = Vector2.Zero,
                Depth = 100f
            };
        }

        public static void Update(float deltaTime)
        {
            s_roots.Update();

            foreach (UIRoot root in s_roots)
                root.Update(deltaTime);
        }

        public static void Draw()
        {
            Layer layer = Layers.UI;

            Graphics.BeginLayer(layer);

            foreach (UIRoot root in s_roots)
                root.Draw();

            if (CursorMode != CursorMode.Hidden)
            {
                Vector2 mousePosition = MousePosition;
                s_cursorSprite.Draw(mousePosition);
            }

            Graphics.EndCurrentLayer();
        }

        public static void AddRoot(UIRoot root)
        {
            s_roots.Add(root);
        }

        public static void RemoveRoot(UIRoot root)
        {
            s_roots.Remove(root);
        }
    }
}
