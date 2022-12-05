using OpenTK.Audio.OpenAL;

namespace SlowAndReverb
{
    public abstract class SoundPlayer
    {
        private readonly byte[] _data;

        private readonly ALFormat _format;
        private readonly int _sampleRate;

        private ALSourceState _sourceState = ALSourceState.Initial;

        public SoundPlayer(WaveFile file)
        {
            _data = file.Buffer.ToArray();

            _format = file.Format;
            _sampleRate = file.SampleRate;
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
        public bool Prioritized { get; set; }

        public ALFormat Format => _format;
        public int SampleRate => _sampleRate;

        protected SoundSource Source { get; set; }

        protected float VolumeCached { get; set; }
        protected float PitchCached { get; set; }
        protected Vector2 PositionCached { get; set; }

        protected byte[] Data => _data;
        protected ALSourceState SourceState => _sourceState;

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
                _sourceState = Source.GetState();
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
