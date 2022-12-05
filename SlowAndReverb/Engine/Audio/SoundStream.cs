using OpenTK.Audio.OpenAL;

namespace SlowAndReverb
{
    public class SoundStream : SoundPlayer
    {
        private const int BuffersCount = 4;
        private const int ChunkSize = 65536;

        private SoundState _state = SoundState.Stopped;

        private int[] _bufferHandles;
        private int _unqueuedBuffersCount;

        private int _cursor;

        private bool _looping;

        public SoundStream(WaveFile file) : base(file)
        {

        }

        public override bool Looping
        {
            get
            {
                return _looping;
            }

            set
            {
                _looping = value;
            }
        }

        public static bool CanBeStreamed(IEnumerable<byte> data)
        {
            return data.Count() > ChunkSize * BuffersCount;
        }

        public override void Play()
        {
            if (_state == SoundState.Playing)
                return;

            if (_state == SoundState.Stopped)
            {
                base.Play();

                _bufferHandles = AL.GenBuffers(BuffersCount);

                for (int i = 0; i < BuffersCount; i++)
                {
                    int bytesLeft = Data.Length - _cursor;
                    int length = LimitBufferLength(bytesLeft);
                    int position = Math.Min(_cursor, Data.Length - 1);

                    SetBufferData(_bufferHandles[i], Data, position, length);

                    _cursor += length;
                }

                Source.QueueBuffers(_bufferHandles);
            }

            Source.Play();

            _state = SoundState.Playing;
        }

        public override void Update()
        {
            if (_state != SoundState.Playing)
                return;

            base.Update();

            int buffersProcessed = Source.GetProcessedBuffersCount();
            _unqueuedBuffersCount += buffersProcessed;

            for (int i = 0; i < buffersProcessed; i++)
            {
                int bufferHandle = Source.UnqueueBuffer();
                int bytesLeft = Data.Length - _cursor;

                if (bytesLeft <= 0)
                    continue;

                var bufferData = new byte[ChunkSize];
                int length = LimitBufferLength(bytesLeft);
                int dataLength = length;

                Array.Copy(Data, _cursor, bufferData, 0, length);

                if (bytesLeft <= ChunkSize && _looping)
                {
                    int offset = ChunkSize - length;

                    if (offset > 0)
                        Array.Copy(Data, 0, bufferData, length, offset);

                    dataLength += offset;
                    _cursor = offset;
                }
                else
                {
                    _cursor += length;
                }

                SetBufferData(bufferHandle, bufferData, 0, dataLength);
                Source.QueueBuffer(bufferHandle);

                _unqueuedBuffersCount--;
            }

            if (_unqueuedBuffersCount >= BuffersCount)
                OnStopped();
            else if (SourceState != ALSourceState.Playing)
                Source.Play();
        }

        public override void Pause()
        {
            if (_state != SoundState.Playing)
                return;

            base.Pause();

            _state = SoundState.Paused;
        }

        public override void Stop()
        {
            if (_state == SoundState.Stopped)
                return;

            base.Stop();
        }

        public override SoundState GetState()
        {
            return _state;
        }

        protected override void OnStopped()
        {
            int queuedBuffersLeft = BuffersCount - _unqueuedBuffersCount;

            if (queuedBuffersLeft > 0)
                Source.UnqueueBuffers(queuedBuffersLeft);

            AL.DeleteBuffers(_bufferHandles);
            base.OnStopped();

            _cursor = 0;

            _state = SoundState.Stopped;
        }

        private void SetBufferData(int handle, byte[] source, int position, int length)
        {
            AL.BufferData(handle, Format, ref source[position], length, SampleRate);
        }

        private int LimitBufferLength(int length)
        {
            return Math.Min(length, ChunkSize);
        }
    }
}
