using OpenTK.Audio.OpenAL;
using System.Collections.Generic;

namespace Carpet
{
    public sealed class SoundDevice
    {
        public SoundDevice(string name)
        {
            Name = name;
        }

        public string Name { get; private init; }
        public bool Opened { get; private set; }
        public ALDevice Handle { get; private set; }

        public static IEnumerable<string> GetAvailableDevicesNames()
        {
            return ALC.GetStringList(GetEnumerationStringList.DeviceSpecifier);
        }

        public void Open()
        {
            if (Opened)
                return;

            Handle = ALC.OpenDevice(Name);

            if (Handle == ALDevice.Null)
            {
                // TODO: Throw exception?
            }

            Opened = true;
        }

        public void Close()
        {
            if (!Opened)
                return;

            ALC.CloseDevice(Handle);

            Opened = false;
        }
    }
}
