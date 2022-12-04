using OpenTK.Audio.OpenAL;

namespace SlowAndReverb
{
    public static class SFX
    {
        private static AudioContext s_currentContext;

        private static readonly List<SoundSource> s_freeSources = new List<SoundSource>();
        private static readonly Dictionary<Sound, SoundSource> s_soundPool = new Dictionary<Sound, SoundSource>();

        private static readonly int s_maxSources = 32;

        public static AudioContext CurrentContext => s_currentContext;

        public static void Initialize(string deviceName)
        {
            if (s_currentContext is not null)
            {
                s_currentContext.Device.Close();
                s_currentContext.Destroy();
            }

            s_currentContext = new AudioContext(deviceName);
            s_currentContext.MakeCurrent();

            s_freeSources.Clear();

            ALContextAttributes attributes = s_currentContext.GetAttributes();
            int sourcesCount = Math.Min(attributes.MonoSources.Value, s_maxSources);

            for (int i = 0; i < sourcesCount; i++)
                s_freeSources.Add(new SoundSource());
        }

        public static void Update()
        {
            foreach (Sound sound in s_soundPool.Keys)
                sound.Update();
        }

        public static Sound PlaySound(string name, float initialVolume, float initialPitch)
        {
            var sound = new Sound(name, initialVolume, initialPitch);

            sound.Play();

            return sound;
        }

        public static Sound PlaySound(string name)
        {
            return PlaySound(name, 1f, 1f);
        }

        public static SoundSource AllocateSource(Sound sound)
        {
            SoundSource freeSource = s_freeSources.FirstOrDefault();

            if (freeSource is not null)
            {
                s_freeSources.Remove(freeSource);
                s_soundPool.Add(sound, freeSource);

                return freeSource;
            }

            foreach (Sound pooledSound in s_soundPool.Keys)
            {
                if (pooledSound.Prioritized)
                    continue;

                pooledSound.Stop();

                SoundSource source = s_soundPool[pooledSound];

                s_soundPool.Remove(pooledSound);
                s_soundPool.Add(sound, source);

                return source;
            }

            return null;
        }

        public static void FreeSound(Sound sound)
        {
            if (!s_soundPool.ContainsKey(sound))
                return;

            SoundSource source = s_soundPool[sound];

            s_soundPool.Remove(sound);
            s_freeSources.Add(source);
        }
    }
}
