using OpenTK.Audio.OpenAL;
using System;
using System.Collections.Generic;
using System.IO;

namespace SlowAndReverb
{
    public sealed class WaveFile
    {
        private const string WaveSignature = "RIFF";
        private const string WaveFormat = "WAVE";
        private const string WaveFormatSignature = "fmt ";
        private const string WaveDataSignature = "data";

        private readonly byte[] _data;

        private readonly ALFormat _format;
        private readonly int _sampleRate;

        public WaveFile(Stream stream)
        {
            using (BinaryReader reader = new BinaryReader(stream))
            {
                string signature = ReadWaveFileProperty(reader);
                int riffChunkSize = reader.ReadInt32();

                string formatName = ReadWaveFileProperty(reader);

                if (signature != WaveSignature || formatName != WaveFormat)
                    throw new NotSupportedException("The stream is not a wave file");

                string formatSignature = ReadWaveFileProperty(reader);

                int formatChunkSize = reader.ReadInt32();

                short audioFormat = reader.ReadInt16();
                short channels = reader.ReadInt16();

                _sampleRate = reader.ReadInt32();

                int byteRate = reader.ReadInt32();

                short blockAlign = reader.ReadInt16();
                short bitsPerSample = reader.ReadInt16();

                string dataSignature = ReadWaveFileProperty(reader);

                if (formatSignature != WaveFormatSignature || dataSignature != WaveDataSignature)
                    throw new NotSupportedException("This wave file is not supported");

                int dataChunkSize = reader.ReadInt32();

                _data = reader.ReadBytes((int)reader.BaseStream.Length);
                _format = GetWaveFormat(channels, bitsPerSample);
            }
        }

        public IEnumerable<byte> Buffer => _data;
        public ALFormat Format => _format;
        public int SampleRate => _sampleRate;

        private static string ReadWaveFileProperty(BinaryReader reader)
        {
            return new string(reader.ReadChars(4));
        }

        private static ALFormat GetWaveFormat(int channels, int bitsPerSample)
        {
            if (channels == 1)
            {
                if (bitsPerSample == 8)
                    return ALFormat.Mono8;

                return ALFormat.Mono16;
            }
            else if (channels == 2)
            {
                if (bitsPerSample == 8)
                    return ALFormat.Stereo8;

                return ALFormat.Stereo16;
            }

            throw new NotSupportedException("This format is not supported");
        }
    }
}
