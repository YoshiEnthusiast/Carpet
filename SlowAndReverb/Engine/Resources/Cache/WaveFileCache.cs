using OpenTK.Audio.OpenAL;
using System.IO;

namespace SlowAndReverb
{
    public class WaveFileCache : FileCache<WaveFile>
    {
        public WaveFileCache(string mainDirectory, bool load) : base(".wav", mainDirectory, load)
        {

        }

        protected override WaveFile CreateItem(Stream stream)
        {
            return new WaveFile(stream);
        }
    }
}
