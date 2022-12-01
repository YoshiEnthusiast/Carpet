using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Xml;
using System.Xml.Linq;

namespace SlowAndReverb
{
    public static class Content
    {
        private const string WaveSignature = "RIFF";
        private const string WaveFormat = "WAVE";
        private const string WaveFormatSignature = "fmt ";
        private const string WaveDataSignature = "data";

        private const string DefaultShaderName = "default";

        private static readonly Dictionary<string, Texture> s_textures = new Dictionary<string, Texture>();
        private static readonly Dictionary<string, string> s_shadersSource = new Dictionary<string, string>();
        private static readonly Dictionary<string, ShaderProgram> s_shaderPrograms = new Dictionary<string, ShaderProgram>();
        private static readonly Dictionary<string, FontFamily> s_fontFamilies = new Dictionary<string, FontFamily>();

        private static readonly List<WaveFile> s_sounds = new List<WaveFile>();

        public static IEnumerable<WaveFile> Sounds => s_sounds;

        public static Texture GetTexture(string fileName)
        {
            string path = Paths.GetTexturePath(fileName);

            if (s_textures.TryGetValue(path, out Texture cachedTexture))
                return cachedTexture;

            Texture texture = LoadTexture(path);
            AddTexture(path, texture);

            return texture;
        }

        public static ShaderProgram GetShaderProgram(string vertexFileName, string fragmentFileName, string geometryFileName)
        {
            string vertexPath = Paths.GetVertexShaderPath(vertexFileName);  
            string fragmentPath = Paths.GetFragmentShaderPath(fragmentFileName);
            string geometryPath = null;

            var nameBuilder = new StringBuilder();

            nameBuilder.Append(vertexPath);
            nameBuilder.Append(fragmentPath);

            if (geometryFileName is not null)
            {
                geometryPath = GetGeometryShaderSource(geometryPath);

                nameBuilder.Append(geometryPath);
            }

            string programName = nameBuilder.ToString();

            if (s_shaderPrograms.TryGetValue(programName, out ShaderProgram shaderProgram))
                return shaderProgram;
            
            var newProgram = new ShaderProgram(GetShaderSource(vertexPath), GetShaderSource(fragmentPath), GetShaderSource(geometryPath));

            s_shaderPrograms.Add(programName, newProgram);

            return newProgram;
        }

        public static FontFamily GetFontFamily(string fileName)
        {
            string path = Paths.GetFontPath(fileName);

            if (s_fontFamilies.TryGetValue(path, out FontFamily chachedFont))
                return chachedFont;

            FontFamily font = LoadFontFamily(path);
            AddFontFamily(path, font);

            return font;
        }

        public static ShaderProgram GetShaderProgram(string vertexFileName, string fragmentFileName)
        {
            return GetShaderProgram(vertexFileName, fragmentFileName, null);
        }

        public static ShaderProgram GetShaderProgram(string fragmentFileName)
        {
            return GetShaderProgram(DefaultShaderName, fragmentFileName);   
        }

        public static string GetVertexShaderSource(string fileName)
        {
            return GetShaderSource(Paths.GetVertexShaderPath(fileName));
        }

        public static string GetFragmentShaderSource(string fileName)
        {
            return GetShaderSource(Paths.GetFragmentShaderPath(fileName));
        }

        public static string GetGeometryShaderSource(string fileName)
        {
            return GetShaderSource(Paths.GetGeometryShaderPath(fileName));
        }

        public static void LoadEverything()
        {
            s_textures.Clear(); 
            s_shadersSource.Clear();

            ApplyToAllFiles(Paths.TexturesStorage, AddTexture);

            ApplyToAllFiles(Paths.VertexShadersStorage, AddShaderSource);
            ApplyToAllFiles(Paths.FragmentShadersStorage, AddShaderSource);
            ApplyToAllFiles(Paths.GeometryShadersStorage, AddShaderSource);
            ApplyToAllFiles(Paths.FontsStorage, AddFontFamily);
            ApplyToAllFiles(Paths.SoundsStorage, AddSound);
        }

        private static string GetShaderSource(string path)
        {
            if (path is null)
                return null;

            if (s_shadersSource.TryGetValue(path, out string cachedSource))
                return cachedSource;

            string source = ReadShaderSource(path);
            AddShaderSource(path, source);

            return source;
        }

        private static void ApplyToAllFiles(ContentStorage storage, Action<string> action)
        {
            ApplyToAllFiles(storage.Directory, storage.Extension, action);
        }

        private static void ApplyToAllFiles(string directory, string extension, Action<string> action)
        {
            foreach (string fileName in Directory.GetFiles(directory))
                if (Path.GetExtension(fileName) == extension)
                    action(fileName);

            foreach (string subDirectory in Directory.GetDirectories(directory))
                ApplyToAllFiles(subDirectory, extension, action);
        }

        private static void AddTexture(string path)
        {
            AddTexture(path, LoadTexture(path));    
        }

        private static void AddTexture(string path, Texture texture)
        {
            s_textures.Add(path, texture);
        }

        private static void AddShaderSource(string path)
        {
            AddShaderSource(path, ReadShaderSource(path));   
        }

        private static void AddShaderSource(string path, string source)
        {
            s_shadersSource.Add(path, source);
        }

        private static void AddFontFamily(string path)
        {
            AddFontFamily(path, LoadFontFamily(path));
        }

        private static void AddFontFamily(string path, FontFamily font)
        {
            s_fontFamilies.Add(path, font);
        }

        private static void AddSound(string path)
        {
            using (BinaryReader reader = new BinaryReader(File.OpenRead(path)))
            {
                string signature = ReadWaveFileProperty(reader);
                int riffChunkSize = reader.ReadInt32();

                string format = ReadWaveFileProperty(reader);

                if (signature != WaveSignature || format != WaveFormat)
                    throw new NotSupportedException("The stream is not a wave file");

                string formatSignature = ReadWaveFileProperty(reader);

                int formatChunkSize = reader.ReadInt32();

                short audioFormat = reader.ReadInt16();
                short channels = reader.ReadInt16();

                int sampleRate = reader.ReadInt32();
                int byteRate = reader.ReadInt32();

                short blockAlign = reader.ReadInt16();
                short bitsPerSample = reader.ReadInt16();

                string dataSignature = ReadWaveFileProperty(reader);

                if (formatSignature != WaveFormatSignature || dataSignature != WaveDataSignature)
                    throw new NotSupportedException("This wave file is not supported");

                int dataChunkSize = reader.ReadInt32();

                byte[] buffer = reader.ReadBytes((int)reader.BaseStream.Length);

                s_sounds.Add(new WaveFile(Path.GetFileNameWithoutExtension(path), buffer, channels, bitsPerSample, sampleRate));
            }
        }

        private static string ReadWaveFileProperty(BinaryReader reader)
        {
            return new string(reader.ReadChars(4));
        }

        private static Texture LoadTexture(string path)
        {
            return Texture.FromFile(path);
        }

        private static FontFamily LoadFontFamily(string fontPath)
        {
            string dataFileName = Paths.ConcatenatePaths(Path.GetDirectoryName(fontPath), Path.GetFileNameWithoutExtension(fontPath) + ".xml");

            if (!File.Exists(dataFileName))
                return null;

            return new FontFamily(fontPath, dataFileName);
        }

        private static string ReadShaderSource(string path)
        {
            using (StreamReader streamReader = new StreamReader(path))
                return streamReader.ReadToEnd();
        }
    }
}
