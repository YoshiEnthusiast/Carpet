using OpenTK.Audio.OpenAL;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Carpet
{
    public static class SFX
    {
        private static readonly List<SoundSource> s_freeSources = new List<SoundSource>();
        private static readonly Dictionary<SoundPlayer, SoundSource> s_soundPool = new Dictionary<SoundPlayer, SoundSource>();

        private static readonly int s_maxSources = 32;

        public static AudioContext CurrentContext { get; private set; }

        internal static void Initialize(string deviceName)
        {
            if (CurrentContext is not null)
            {
                CurrentContext.Device.Close();
                CurrentContext.Destroy();
            }

            CurrentContext = new AudioContext(deviceName);
            CurrentContext.MakeCurrent();

            s_freeSources.Clear();

            ALContextAttributes attributes = CurrentContext.GetAttributes();
            int sourcesCount = Maths.Min(attributes.MonoSources.Value, s_maxSources);

            for (int i = 0; i < sourcesCount; i++)
                s_freeSources.Add(new SoundSource());
        }

        public static void Update()
        {
            foreach (SoundPlayer sound in s_soundPool.Keys)
                sound.Update();
        }

        public static SoundEffect PlaySound(string name, float initialVolume, float initialPitch)
        {
            var sound = new SoundEffect(name, initialVolume, initialPitch);

            sound.Play();

            return sound;
        }

        public static SoundEffect PlaySound(string name)
        {
            return PlaySound(name, 1f, 1f);
        }

        public static SoundSource AllocateSource(SoundPlayer sound)
        {
            SoundSource freeSource = s_freeSources.FirstOrDefault();

            if (freeSource is not null)
            {
                s_freeSources.Remove(freeSource);
                s_soundPool.Add(sound, freeSource);

                return freeSource;
            }

            foreach (SoundPlayer pooledSound in s_soundPool.Keys)
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

        public static void FreeSound(SoundPlayer sound)
        {
            if (!s_soundPool.ContainsKey(sound))
                return;

            SoundSource source = s_soundPool[sound];

            s_soundPool.Remove(sound);
            s_freeSources.Add(source);
        }

        public static void SetListenerPosition(Vector2 position)
        {
            AL.Listener(ALListener3f.Position, position.X, position.Y, 0f);
        }

        public static void SetListenerVelocity(Vector2 position)
        {
            AL.Listener(ALListener3f.Velocity, position.X, position.Y, 0f);
        }
    }
}
