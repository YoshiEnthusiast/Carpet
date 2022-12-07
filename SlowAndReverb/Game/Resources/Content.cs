using System.Text;

namespace SlowAndReverb
{
    public static class Content
    {
        public const string DefaultShaderName = "default";

        private static TextureCache s_textureCache;
        private static FontFamilyCache s_fontFamilyCache;
        private static WaveFileCache s_waveFileCache;

        private static ShaderSourceCache s_vertexSourceCache;
        private static ShaderSourceCache s_fragmentSourceCache;
        private static ShaderSourceCache s_geometrySourceCache;

        private static readonly Dictionary<string, ShaderProgram> s_shaders = new Dictionary<string, ShaderProgram>();

        public static void LoadEverything()
        {
            s_textureCache = new TextureCache("Textures", true);
            s_fontFamilyCache = new FontFamilyCache("Fonts", true);
            s_waveFileCache = new WaveFileCache("SFX", true);

            s_vertexSourceCache = new ShaderSourceCache("Shaders/Vertex", ".vert", true);
            s_fragmentSourceCache = new ShaderSourceCache("Shaders/Fragment", ".frag", true);
            s_geometrySourceCache = new ShaderSourceCache("Shaders/Geometry", ".geom", true);

            s_shaders.Clear();
        }

        public static ShaderProgram GetShaderProgram(string vertexName, string fragmentName, string geometryName)
        {
            string vertexPath = s_vertexSourceCache.GetPath(vertexName);
            string fragmentPath = s_fragmentSourceCache.GetPath(fragmentName);
            string geometryPath = null;

            var nameBuilder = new StringBuilder();

            nameBuilder.Append(vertexPath);
            nameBuilder.Append(fragmentPath);

            if (geometryName is not null)
            {
                geometryPath = s_geometrySourceCache.GetPath(geometryPath);

                nameBuilder.Append(geometryPath);
            }

            string programName = nameBuilder.ToString();

            if (s_shaders.TryGetValue(programName, out ShaderProgram shaderProgram))
                return shaderProgram;

            string vertexSource = s_vertexSourceCache.GetItem(vertexPath);
            string fragmentSource = s_fragmentSourceCache.GetItem(fragmentPath);
            string geometrySource = geometryPath is null ? null : s_geometrySourceCache.GetItem(geometryPath);

            var newProgram = new ShaderProgram(vertexSource, fragmentSource, geometrySource);

            s_shaders.Add(programName, newProgram);

            return newProgram;
        }

        public static ShaderProgram GetShaderProgram(string vertexName, string fragmentName)
        {
            return GetShaderProgram(vertexName, fragmentName, null);
        }

        public static ShaderProgram GetShaderProgram(string fragmentName)
        {
            return GetShaderProgram(DefaultShaderName, fragmentName);
        }

        public static Texture GetTexture(string fileName)
        {
            return s_textureCache.GetItem(fileName);    
        }

        public static FontFamily GetFontFamily(string fileName)
        {
            return s_fontFamilyCache.GetItem(fileName); 
        }

        public static WaveFile GetWaveFile(string fileName)
        {
            return s_waveFileCache.GetItem(fileName);
        }
    }
}
