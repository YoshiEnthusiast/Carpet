namespace SlowAndReverb
{
    public class SoundEffect
    {
        private readonly SoundPlayer _player;   

        public SoundEffect(string name, float initialVolume, float initialPitch)
        {
            WaveFile file = Content.GetWaveFile(name);

            _player = CreatePlayer(file);

            _player.Volume = initialVolume;
            _player.Pitch = initialPitch;
        }

        public SoundEffect(string name) : this(name, 1f, 1f)
        {

        }

        public virtual float Volume
        {
            get
            {
                return _player.Volume;  
            }

            set
            {
                _player.Volume = value;
            }
        }

        public virtual float Pitch
        {
            get
            {
                return _player.Pitch;
            }

            set
            {
                _player.Pitch = value;
            }
        }

        public virtual bool Looping
        {
            get
            {
                return _player.Looping;
            }

            set
            {
                _player.Looping = value;
            }
        }

        public virtual Vector2 Position
        {
            get
            {
                return _player.Position;
            }

            set
            {
                _player.Position = value;
            } 
        }

        public bool Prioritized
        {
            get
            {
                return _player.Prioritized;
            }

            set
            {
                _player.Prioritized = value;
            }
        }

        public virtual void Play()
        {
            _player.Play();
        }

        public virtual void Pause()
        {
            _player.Pause();
        }

        public virtual void Stop()
        {
            _player.Stop();
        }

        public SoundState GetState()
        {
            return _player.GetState();
        }

        private SoundPlayer CreatePlayer(WaveFile file)
        {
            if (SoundStream.CanBeStreamed(file.Buffer))
                return new SoundStream(file);

            return new DefaultSoundPlayer(file);
        }
    }
}
