using OpenTK.Audio.OpenAL;

namespace SlowAndReverb
{
    public static class SFX
    {
        private static AudioContext s_currentContext;

        private static readonly Dictionary<string, SoundBuffer> s_buffers = new Dictionary<string, SoundBuffer>();
        private static readonly List<SoundSource> s_sources = new List<SoundSource>();

        private static readonly int s_maxSources = 32;

        public static void Initialize(string deviceName)
        {
            if (s_currentContext is not null)
            {
                s_currentContext.Device.Close();
                s_currentContext.Destroy();
            }

            s_currentContext = new AudioContext(deviceName);
            s_currentContext.MakeCurrent();

            s_buffers.Clear();
            s_sources.Clear();

            ALContextAttributes attributes = s_currentContext.GetAttributes();
            int sourcesCount = Math.Min(attributes.MonoSources.Value, s_maxSources);

            foreach (WaveFile waveFile in Content.Sounds)
                s_buffers.Add(waveFile.Name, new SoundBuffer(waveFile));

            for (int i = 0; i < sourcesCount; i++)
                s_sources.Add(new SoundSource());
        }

        public static Sound PlaySound(string name, float initialVolume, float initialPitch, bool looping)
        {
            var sound = new Sound(name, initialVolume, initialPitch, looping);

            sound.Play();

            return sound;
        }

        public static Sound PlaySound(string name)
        {
            return PlaySound(name, 1f, 1f, false);
        }

        public static SoundBuffer GetBuffer(string name)
        {
            if (s_buffers.TryGetValue(name, out SoundBuffer buffer))
                return buffer;

            return null;
        }

        public static SoundSource GetVacantSource()
        {
            foreach (SoundSource source in s_sources)
            {
                ALSourceState state = source.GetState();

                if (state == ALSourceState.Initial || state == ALSourceState.Stopped)
                    return source;
            }

            // Throw an exception???
            // Stop already playing sounds depending on their priority?????????

            return null;
        }

        public static AudioContext CurrentContext => s_currentContext;
    }
}
