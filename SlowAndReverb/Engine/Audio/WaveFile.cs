using OpenTK.Audio.OpenAL;

namespace SlowAndReverb
{
    public sealed record class WaveFile(IEnumerable<byte> Buffer, ALFormat Format, int SampleRate);
}
