using OpenTK.Audio.OpenAL;

namespace SlowAndReverb
{
    public sealed class AudioContext
    {
        private readonly ALContext _context;

        public unsafe AudioContext(string deviceName)
        {
            Device = new SoundDevice(deviceName);
            Device.Open();

            _context = ALC.CreateContext(Device.Handle, (int*)null);
        }

        public SoundDevice Device { get; private init; }
        public bool Destroyed { get; private set; }  

        public void MakeCurrent()
        {
            if (Destroyed)
                return;

            ALC.MakeContextCurrent(_context);
        }

        public ALContextAttributes GetAttributes()
        {
            return ALC.GetContextAttributes(Device.Handle);
        }

        public void Destroy()
        {
            if (Destroyed)
                return;

            ALC.DestroyContext(_context);

            Destroyed = true;
        }
    }
}
