using OpenTK.Audio.OpenAL;

namespace SlowAndReverb
{
    public class Sound
    {
        private readonly byte[] _data;

        private readonly ALFormat _format;
        private readonly int _sampleRate;

        private readonly int _buffersCount = 4;
        private readonly int _chunkSize = 65536;

        private SoundSource _source;

        private SoundState _state = SoundState.Stopped;

        private int[] _bufferHandles;
        private int _unqueuedBuffersCount;

        private int _cursor;

        private Vector2 _position;
        private float _volume;
        private float _pitch;

        public Sound(string fileName, float initialVolume, float initialPitch)
        {
            WaveFile file = Content.GetWaveFile(fileName);

            _data = file.Buffer.ToArray();

            _format = file.Format;
            _sampleRate = file.SampleRate;

            _volume = initialVolume;    
            _pitch = initialPitch;
        }

        public Sound(string name) : this(name, 1f, 1f)
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
                _source?.SetVolume(value);

                _volume = value;
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
                _source?.SetPitch(value);

                _pitch = value; 
            }
        }

        public Vector2 Position
        {
            get
            {
                return _position;
            }

            set
            {
                _source?.SetPosition(value);

                _position = value;  
            }
        }

        public bool Prioritized { get; set; }
        public bool Looping { get; set; }

        public SoundState State => _state;  
        public ALFormat Format => _format; 
        public int SampleRate => _sampleRate;

        public virtual void Play()
        {
            if (_state == SoundState.Playing)
                return;

            if (_state == SoundState.Stopped)
            {
                _source = SFX.AllocateSource(this);

                if (_source is null)
                    return;

                _source.SetVolume(_volume);
                _source.SetPitch(_pitch);
                _source.SetPosition(_position);

                _bufferHandles = AL.GenBuffers(_buffersCount);

                for (int i = 0; i < _buffersCount; i++)
                {
                    int bytesLeft = _data.Length - _cursor;
                    int length = LimitBufferLength(bytesLeft);
                    int position = Math.Min(_cursor, _data.Length - 1);

                    SetBufferData(_bufferHandles[i], _data, position, length);

                    _cursor += length;
                }

                _source.QueueBuffers(_bufferHandles);
            }

            _source.Play();

            _state = SoundState.Playing;
        }

        public virtual void Update()
        {
            if (_state != SoundState.Playing)
                return;

            int buffersProcessed = _source.GetProcessedBuffersCount();
            _unqueuedBuffersCount += buffersProcessed;

            for (int i = 0; i < buffersProcessed; i++)
            {
                int bufferHandle = _source.UnqueueBuffer();
                int bytesLeft = _data.Length - _cursor;

                if (bytesLeft <= 0)
                    continue;

                var bufferData = new byte[_chunkSize];
                int length = LimitBufferLength(bytesLeft);
                int dataLength = length;

                Array.Copy(_data, _cursor, bufferData, 0, length);

                if (bytesLeft <= _chunkSize && Looping)
                {
                    int offset = _chunkSize - length;

                    if (offset > 0)
                        Array.Copy(_data, 0, bufferData, length, offset);

                    dataLength += offset;
                    _cursor = offset;
                }
                else
                {
                    _cursor += length;
                }

                SetBufferData(bufferHandle, bufferData, 0, dataLength);
                _source.QueueBuffer(bufferHandle);

                _unqueuedBuffersCount--;
            }

            if (_unqueuedBuffersCount >= _buffersCount)
                OnStopped();
            else if (_source.GetState() != ALSourceState.Playing)
                _source.Play();
        }

        public virtual void Pause()
        {
            if (_state != SoundState.Playing)
                return;

            _source.Pause();

            _state = SoundState.Paused;
        }

        public virtual void Stop()
        {
            if (_state == SoundState.Stopped)
                return; 

            _source.Stop();

            OnStopped();
        }

        private void OnStopped()
        {
            int queuedBuffersLeft = _buffersCount - _unqueuedBuffersCount;

            if (queuedBuffersLeft > 0)
                _source.UnqueueBuffers(queuedBuffersLeft);

            AL.DeleteBuffers(_bufferHandles);
            SFX.FreeSound(this);

            _cursor = 0;

            _state = SoundState.Stopped;
        }

        private void SetBufferData(int handle, byte[] source, int position, int length)
        {
            AL.BufferData(handle, _format, ref source[position], length, _sampleRate);
        }

        private int LimitBufferLength(int length)
        {
            return Math.Min(length, _chunkSize);
        }
    }
}
