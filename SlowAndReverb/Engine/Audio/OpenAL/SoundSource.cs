using OpenTK.Audio.OpenAL;

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

        public void SetBuffer(int handle)
        {
            AL.Source(Handle, ALSourcei.Buffer, handle);
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

        public void SetPosition(Vector2 position)
        {
            AL.Source(Handle, ALSource3f.Position, position.X, position.Y, 0f);
        }

        public void SetVelocity(Vector2 velocity)
        {
            AL.Source(Handle, ALSource3f.Velocity, velocity.X, velocity.Y, 0f);
        }

        public void SetDirection(Vector2 direction)
        {
            AL.Source(Handle, ALSource3f.Direction, direction.X, direction.Y, 0f);
        }

        public void QueueBuffers(int[] bufferHandles)
        {
            AL.SourceQueueBuffers(Handle, bufferHandles.Length, bufferHandles);
        }

        public void UnqueueBuffers(int count)
        {
            AL.SourceUnqueueBuffers(Handle, count);
        }

        public void QueueBuffer(int bufferHandle)
        {
            AL.SourceQueueBuffer(Handle, bufferHandle); 
        }

        public int UnqueueBuffer()
        {
            return AL.SourceUnqueueBuffer(Handle);
        }

        public int GetProcessedBuffersCount()
        {
            AL.GetSource(Handle, ALGetSourcei.BuffersProcessed, out int count);

            return count;
        }

        public int GetQueuedBuffersCount()
        {
            AL.GetSource(Handle, ALGetSourcei.BuffersQueued, out int count);

            return count;
        }
    }
}
