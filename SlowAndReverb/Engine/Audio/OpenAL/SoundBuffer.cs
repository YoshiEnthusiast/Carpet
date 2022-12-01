using OpenTK.Audio.OpenAL;
using System.Runtime.CompilerServices;

namespace SlowAndReverb
{
    public sealed class SoundBuffer : OpenALObject
    {
        public unsafe SoundBuffer(WaveFile file)
        {
            Handle = AL.GenBuffer();

            ALFormat soundFormat = GetFormat(file.Channels, file.BitsPerSample);
            byte[] buffer = file.Buffer.ToArray();

            AL.BufferData(Handle, soundFormat, ref buffer[0], buffer.Length, file.SampleRate);
        }   

        public override void Delete()
        {
            AL.DeleteBuffer(Handle);

            base.Delete();
        }

        private static ALFormat GetFormat(int channels, int bitsPerSample)
        {
            if (channels == 1)
            {
                if (bitsPerSample == 8)
                    return ALFormat.Mono8;

                return ALFormat.Mono16;
            }
            else if (channels == 2)
            {
                if (bitsPerSample == 8)
                    return ALFormat.Stereo8;

                return ALFormat.Stereo16;
            }

            throw new NotSupportedException("This format is not supported");
        }
    }
}
