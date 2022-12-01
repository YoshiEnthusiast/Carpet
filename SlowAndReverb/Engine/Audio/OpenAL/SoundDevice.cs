using OpenTK.Audio.OpenAL;

namespace SlowAndReverb
{
    public sealed class SoundDevice
    {
        private readonly string _name;

        private ALDevice _device;
        private bool _opened;

        public SoundDevice(string name)
        {
            _name = name;
        }

        public string Name => _name;
        public bool Opened => _opened;
        public ALDevice Handle => _device;

        public static IEnumerable<string> GetAvailableDevicesNames()
        {
            return ALC.GetStringList(GetEnumerationStringList.DeviceSpecifier);
        }

        public void Open()
        {
            if (_opened)
                return;

            _device = ALC.OpenDevice(_name);

            if (_device == ALDevice.Null)
            {
                // Throw exception
            }

            _opened = true;
        }

        public void Close()
        {
            if (!_opened)
                return;

            ALC.CloseDevice(_device);   

            _opened = false;
        }
    }
}
