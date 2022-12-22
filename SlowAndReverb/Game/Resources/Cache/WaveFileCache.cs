using OpenTK.Audio.OpenAL;

namespace SlowAndReverb
{
    public class WaveFileCache : Cache<WaveFile>
    {
        public WaveFileCache(string mainDirectory, bool load) : base(".wav", mainDirectory, load)
        {

        }

        protected override WaveFile CreateItem(string path)
        {
            return new WaveFile(path);
        }
    }
}
