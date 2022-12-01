using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SlowAndReverb
{
    public sealed record class WaveFile(string Name, IEnumerable<byte> Buffer, int Channels, int BitsPerSample, int SampleRate);
}
