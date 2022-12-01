using OpenTK.Audio.OpenAL;
using OpenTK.Mathematics;

namespace SlowAndReverb
{
    public sealed class SoundSource : OpenALObject
    {
        public SoundSource()
        {
            Handle = AL.GenSource();
        }

        public void Play()
        {
            AL.SourcePlay(Handle);
        }

        public void Pause()
        {
            AL.SourcePause(Handle);
        }

        public void Stop()
        {
            AL.SourceStop(Handle);
        }

        public override void Delete()
        {
            AL.DeleteSource(Handle);

            base.Delete();
        }

        public ALSourceState GetState()
        {
            return AL.GetSourceState(Handle);
        }

        public void SetBuffer(SoundBuffer buffer)
        {
            AL.Source(Handle, ALSourcei.Buffer, buffer.Handle);
        }

        public void SetVolume(float volume)
        {
            AL.Source(Handle, ALSourcef.Gain, volume);
        }

        public void SetPitch(float pitch)
        {
            AL.Source(Handle, ALSourcef.Pitch, pitch);
        }

        public void SetLooping(bool looping)
        {
            AL.Source(Handle, ALSourceb.Looping, looping);
        }

        public void SetPosition(Vector3 position)
        {
            AL.Source(Handle, ALSource3f.Position, ref position);
        }

        public void SetVelocity(Vector3 velocity)
        {
            AL.Source(Handle, ALSource3f.Velocity, ref velocity);
        }

        public void SetDirection(Vector3 direction)
        {
            AL.Source(Handle, ALSource3f.Direction, ref direction);
        }
    }
}
