using OpenTK.Audio.OpenAL;

namespace SlowAndReverb
{
    public class DefaultSoundPlayer : SoundPlayer
    {
        private int _bufferHandle;
        private bool _loopingCached;

        public DefaultSoundPlayer(WaveFile file) : base(file)
        {

        }

        public override bool Looping
        {
            get
            {
                return _loopingCached;
            }

            set
            {
                Source?.SetLooping(value);

                _loopingCached = value;
            }
        }

        public override void Play()
        {
            SoundState state = GetState();

            if (state == SoundState.Playing)
                return;

            if (state == SoundState.Stopped)
            {
                _bufferHandle = AL.GenBuffer();
                AL.BufferData(_bufferHandle, Format, ref Data[0], Data.Length, SampleRate);

                base.Play();

                Source.SetBuffer(_bufferHandle);
                Source.SetLooping(_loopingCached);
            }

            Source.Play();
        }

        public override void Pause()
        {
            if (GetState() == SoundState.Stopped)
                return;

            base.Pause();
        }

        public override void Stop()
        {
            if (GetState() != SoundState.Stopped)
                return;

            base.Stop();
        }

        public override SoundState GetState()
        {
            if (SourceState == ALSourceState.Initial)
                return SoundState.Stopped;

            return (SoundState)SourceState;
        }

        protected override void OnStopped()
        {
            Source.SetBuffer(0);
            AL.DeleteBuffer(_bufferHandle);

            base.OnStopped();
        }
    }
}
