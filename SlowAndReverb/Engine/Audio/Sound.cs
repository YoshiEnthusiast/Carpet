using OpenTK.Audio.OpenAL;

namespace SlowAndReverb
{
    public class Sound
    {
        private readonly SoundBuffer _buffer;

        private SoundSource _source;

        private float _volume;
        private float _pitch;
        private bool _looping;

        public Sound(string name, float initialVolume, float initialPitch, bool looping)
        {
            _buffer = SFX.GetBuffer(name);

            _volume = initialVolume;
            _pitch = initialPitch;
            _looping = looping;
        }

        public Sound(string name) : this(name, 1f, 1f, false)
        {

        }

        public float Volume
        {
            get
            {
                return _volume;
            }

            set
            {
                _volume = value;    

                _source?.SetVolume(_volume);    
            }
        }

        public float Pitch
        {
            get
            {
                return _pitch;
            }

            set
            {
                _pitch = value;

                _source?.SetPitch(_pitch);
            }
        }

        public bool Looping
        {
            get
            {
                return _looping;
            }

            set
            {
                _looping = value;

                _source?.SetLooping(_looping);
            }
        }

        public void Play()
        {
            if (_source is null)
            {
                _source = SFX.GetVacantSource();

                _source.SetBuffer(_buffer);

                _source.SetVolume(_volume);
                _source.SetPitch(_pitch);
                _source.SetLooping(_looping);
            }

            _source.Play();
        }

        public void Pause()
        {
            _source?.Pause();   
        }

        public void Stop()
        {
            _source?.Stop();
        }

        public SoundState GetState()
        {
            if (_source is null)
                return SoundState.Stopped;

            ALSourceState state = _source.GetState();

            if (state == ALSourceState.Initial)
                return SoundState.Stopped;

            return (SoundState)state;   
        }
    }
}
