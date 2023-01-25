using OpenTK.Audio.OpenAL;
using System.Linq;

namespace SlowAndReverb
{
    public abstract class SoundPlayer
    {
        public SoundPlayer(WaveFile file)
        {
            Data = file.Buffer.ToArray();

            Format = file.Format;
            SampleRate = file.SampleRate;
        }

        public float Volume
        {
            get
            {
                return VolumeCached;
            }

            set
            {
                Source?.SetVolume(value);

                VolumeCached = value;
            }
        }

        public float Pitch
        {
            get
            {
                return PitchCached;
            }

            set
            {
                Source?.SetPitch(value);

                PitchCached = value;
            }
        }

        public Vector2 Position
        {
            get
            {
                return PositionCached;
            }

            set
            {
                Source?.SetPosition(value);

                PositionCached = value;
            }
        }

        public abstract bool Looping { get; set; }

        public ALFormat Format { get; private init; }
        public int SampleRate { get; private init; }

        public bool Prioritized { get; set; }

        protected byte[] Data { get; private init; }
        protected ALSourceState SourceState { get; private set; }

        protected SoundSource Source { get; set; }

        protected float VolumeCached { get; set; }
        protected float PitchCached { get; set; }
        protected Vector2 PositionCached { get; set; }

        public abstract SoundState GetState();

        public virtual void Play()
        {
            Source = SFX.AllocateSource(this);

            Source.SetVolume(VolumeCached);
            Source.SetPitch(PitchCached);
            Source.SetPosition(PositionCached);
        }

        public virtual void Update()
        {
            if (Source is not null)
                SourceState = Source.GetState();
        }

        public virtual void Pause()
        {
            Source?.Pause();
        }

        public virtual void Stop()
        {
            if (Source is not null)
            {
                Source.Stop();

                OnStopped();
            }
        }

        protected virtual void OnStopped()
        {
            SFX.FreeSound(this);
        }
    }
}
