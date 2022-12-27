﻿using OpenTK.Mathematics;
using OpenTK.Windowing.Desktop;
using System;
using System.Collections.Generic;

namespace SlowAndReverb
{
    public class Resolution
    {
        private static readonly IReadOnlyCollection<Resolution> s_supportedResolutions = new Resolution[]
        {
            new Resolution(1280, 720)
        };

        private static GameWindow s_window;

        private static Resolution s_current;
        private static EventHandler _change;

        private readonly Vector2 _size;

        private Resolution(int width, int height)
        {
            _size = new Vector2(width, height);
        }

        public static event EventHandler Change
        {
            add
            {
                _change += value;
            }

            remove
            {
                _change -= value;
            }
        }

        public static IEnumerable<Resolution> SupportedResolutions => s_supportedResolutions;
        public static Resolution Current => s_current;

        public static int CurrentWidth
        {
            get
            {
                if (s_current is null)
                    return 0;

                return s_current.Width;
            }
        }

        public static int CurrentHeight
        {
            get
            {
                if (s_current is null)
                    return 0;

                return s_current.Height;
            }
        }

        public static Vector2 CurrentSize => s_current.Size;

        public Vector2 Size => _size;
        public int Width => (int)_size.X;
        public int Height => (int)_size.Y;

        public static void SetCurrent(Resolution resolution)
        {
            s_current = resolution;

            resolution.Apply();
            _change?.Invoke(null, EventArgs.Empty);
        }

        internal static void Initialize(GameWindow window)
        {
            s_window = window;
        }

        private void Apply()
        {
            s_window.Size = new Vector2i(Width, Height);
        }
    }
}
