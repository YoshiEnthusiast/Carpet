using OpenTK.Audio.OpenAL;

namespace SlowAndReverb
{
    public sealed class AudioContext
    {
        private readonly SoundDevice _device;
        private readonly ALContext _context;

        private bool _destroyed;

        public unsafe AudioContext(string deviceName)
        {
            _device = new SoundDevice(deviceName);
            _device.Open();

            _context = ALC.CreateContext(_device.Handle, (int*)null);
        }

        public SoundDevice Device => _device;
        public bool Destroyed => _destroyed;    

        public void MakeCurrent()
        {
            if (_destroyed)
                return;

            ALC.MakeContextCurrent(_context);
        }

        public ALContextAttributes GetAttributes()
        {
            return ALC.GetContextAttributes(_device.Handle);
        }

        public void Destroy()
        {
            if (_destroyed)
                return;

            ALC.DestroyContext(_context);

            _destroyed = true;
        }
    }
}
