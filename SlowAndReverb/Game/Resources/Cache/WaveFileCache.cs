using OpenTK.Audio.OpenAL;

namespace SlowAndReverb
{
    public class WaveFileCache : Cache<WaveFile>
    {
        public WaveFileCache(string mainDirectory, bool load) : base(mainDirectory, load)
        {
            Extension = ".wav";
        }

        protected override WaveFile CreateItem(string path)
        {
            return new WaveFile(path);
        }
    }
}
