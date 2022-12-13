using System;
using System.Collections.Generic;
using System.Linq;

namespace SlowAndReverb
{
    // TODO: Add null checks to static properties and make this class not garbage
    public class Resolution
    {
        private static readonly IEnumerable<Resolution> s_supportedResolutions = new Resolution[]
        {
            new Resolution(1280, 720)
        };

        private static Resolution s_current;
        private static EventHandler _change;

        private readonly int _width;
        private readonly int _height;   

        private Resolution(int width, int heigth)
        {
            _width = width;
            _height = heigth;
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

        public static int CurrentWidth => s_current.Width;
        public static int CurrentHeight => s_current.Height;

        public static Vector2 CurrentSize => s_current.Size;

        public Vector2 Size => new Vector2(_width, _height);    
        public int Width => _width;
        public int Height => _height;

        public static void SetCurrent(Resolution resolution)
        {
            s_current = resolution;

            _change?.Invoke(null, EventArgs.Empty);
        }

        public static void Initialize()
        {
            SetCurrent(s_supportedResolutions.First());
        }
    }
}
