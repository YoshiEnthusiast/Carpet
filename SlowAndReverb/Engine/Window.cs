using OpenTK.Windowing.Common;
using OpenTK.Windowing.Desktop;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Carpet
{
    // TODO: delete this probably

    public class Window : NativeWindow
    {
        private Action<FrameEventArgs> _update;
        private Action<FrameEventArgs> _render;

        private Action _load;
        private Action _unload;

        private Vector2 _size;

        public Window(NativeWindowSettings settings) : base(settings)
        {

        }

        public event Action<FrameEventArgs> Update
        { 
            add
            {
                _update += value;
            }

            remove
            {
                _update -= value;
            } 
        }

        public event Action<FrameEventArgs> Render
        { 
            add
            {
                _render += value;
            }

            remove
            {
                _render -= value;
            } 
        }

        public event Action Load
        {
            add
            {
                _load += value;
            }

            remove
            {
                _load -= value;
            }
        }

        public event Action Unload
        {
            add
            {
                _unload += value;
            }

            remove
            {
                _unload -= value;
            }
        }

        public int Frequency { get; set; } = 60;

        public void Run()
        {
            _load?.Invoke();


        }
    }
}
