using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Xml;

namespace Carpet
{
    public static class Content
    {
        public const string EncodedFileExtension = ".rsc";
        public const string DefaultShaderName = "default";

        private const string AtlasName = "atlas";
        private const string AtlasFileName = AtlasName + ".png"; // will be rsc
        private const string AtlasDataFileName = AtlasName + ".xml";

        private static TextureCache s_textureCache;
        private static FontFamilyCache s_fontFamilyCache;
        private static WaveFileCache s_waveFileCache;
        private static PaletteCache s_paletteCache;
        private static AsepriteCache s_asepriteCache;

        private static ShaderSourceCache s_vertexSourceCache;
        private static ShaderSourceCache s_fragmentSourceCache;
        private static ShaderSourceCache s_geometrySourceCache;
        private static ShaderSourceCache s_computeSourceCache;

        private static VirtualTexture s_noTexture;

        private static readonly Dictionary<string, PipelineShaderProgram> s_pipelineShaders = new Dictionary<string, PipelineShaderProgram>();
        private static readonly Dictionary<string, ComputeShaderProgram> s_computeShaders = new Dictionary<string, ComputeShaderProgram>();
        private static readonly Dictionary<string, VirtualTexture> s_virtualTextures = new Dictionary<string, VirtualTexture>();

        private static readonly Dictionary<int, int> s_tileFrames = new Dictionary<int, int>();
        private static readonly Dictionary<string, ControllerMapping> s_controllerMappings = new Dictionary<string, ControllerMapping>();

        public static Texture2D AtlasTexture { get; private set; }
        public static XmlElement DefaultInputSettings { get; private set; }
        public static string Folder { get; private set; }

        internal static void Initialize(string folder)
        {
            Folder = folder;

            XmlDocument mappingsDocument = LoadXML("controllerMappings.xml");
            XmlElement mappings = mappingsDocument["Mappings"];

            foreach (XmlElement mapping in mappings)
            {
                string name = mapping.GetAttribute("Name");
                var controllerMapping = new ControllerMapping(mapping);

                s_controllerMappings[name] = controllerMapping;
            }

            DefaultInputSettings = LoadXML("defaultInputSettings.xml")
                .DocumentElement;
        }

        internal static void LoadGraphics(TextureLoadMode mode)
        {
            s_vertexSourceCache = new ShaderSourceCache("Shaders/Vertex", ".vert", true);
            s_fragmentSourceCache = new ShaderSourceCache("Shaders/Fragment", ".frag", true);
            s_geometrySourceCache = new ShaderSourceCache("Shaders/Geometry", ".geom", true);
            s_computeSourceCache = new ShaderSourceCache("Shaders/Compute", ".comp", true);

            s_textureCache = new TextureCache("Textures", true);
            s_paletteCache = new PaletteCache("Palettes", true);
            s_asepriteCache = new AsepriteCache("Textures", true);

            string texturesDirectory = s_textureCache.MainDirectory;

            string atlasFileName = Path.Combine(texturesDirectory, AtlasFileName);
            string atlasDataFileName = Path.Combine(texturesDirectory, AtlasDataFileName);

            if (mode >= TextureLoadMode.CreateAtlas)
            {
                var atlas = new Atlas(OpenGL.MaxTextureSize, 1);

                IEnumerable<CachedItem<Texture2D>> textureCacheItems = s_textureCache.GetAllValues();
                IEnumerable<CachedItem<Aseprite>> asepriteCacheItems = s_asepriteCache.GetAllValues();
                int texturesDirectoryLength = texturesDirectory.Length;

                // TODO: Find a better way to do this
                var asepriteTextures = new List<Texture2D>();

                foreach (CachedItem<Texture2D> item in textureCacheItems)
                {
                    string path = item.Path;
                    string localPath = GetLocalPathWithoutExtension(path, texturesDirectory);

                    if (localPath == AtlasName)
                        continue;

                    atlas.Add(item.Value, localPath);
                }

                foreach (CachedItem<Aseprite> item in asepriteCacheItems)
                {
                    string path = item.Path;
                    string localPath = GetLocalPathWithoutExtension(path, texturesDirectory);

                    if (localPath == AtlasName)
                        continue;

                    Aseprite aseprite = item.Value;

                    int width = aseprite.Width;
                    int height = aseprite.Height;
                    byte[] data = aseprite.Frames[0].RenderedImage;

                    Texture2D texture = Texture2D.FromBytes(width, height, data);

                    asepriteTextures.Add(texture);
                    atlas.Add(texture, localPath);
                }

                atlas.Build(5);

                foreach (CachedItem<Texture2D> item in textureCacheItems)
                {
                    Texture2D texture = item.Value;

                    texture.Bind();
                    texture.Delete();
                }
                
                foreach (Texture2D texture in asepriteTextures)
                {
                    texture.Bind();
                    texture.Delete();
                }

                s_textureCache.Clear();

                Texture2D atlasTexture = atlas.Texture;
                XmlDocument atlasData = atlas.Data;

                AtlasTexture = atlasTexture;

                if (mode == TextureLoadMode.SaveAtlas)
                {
                    atlasTexture.SaveAsPng(atlasFileName);
                    atlasData.Save(atlasDataFileName);
                }

                LoadVirtualTextures(atlasTexture, atlasData);
            }
            else
            {
                Texture2D atlasTexture = GetTexture(atlasFileName);
                XmlDocument atlasData = Utilities.LoadXML(atlasDataFileName);

                AtlasTexture = atlasTexture;

                LoadVirtualTextures(atlasTexture, atlasData);
            }

            s_fontFamilyCache = new FontFamilyCache("Fonts", true);
            s_noTexture = FindVirtualTexture("noTexture");

            s_pipelineShaders.Clear();
        }

        internal static void Load()
        {
            s_waveFileCache = new WaveFileCache("SFX", true);

            XmlDocument tilesDocument = LoadXML("tiles.xml");
            XmlElement tiles = tilesDocument["Tiles"];

            foreach (XmlElement tile in tiles)
            {
                int mask = tile.GetIntAttribute("Mask");
                int frame = tile.GetIntAttribute("Frame");

                s_tileFrames[mask] = frame;
            }

            // Levels
            // Save data????
        }

        public static PipelineShaderProgram GetPipelineShaderProgram(string vertexName, string fragmentName, string geometryName)
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

            if (s_pipelineShaders.TryGetValue(programName, out PipelineShaderProgram shaderProgram))
                return shaderProgram;

            string vertexSource = s_vertexSourceCache.GetItem(vertexPath);
            string fragmentSource = s_fragmentSourceCache.GetItem(fragmentPath);
            string geometrySource = geometryPath is null ? null : s_geometrySourceCache.GetItem(geometryPath);

            var newProgram = new PipelineShaderProgram(vertexSource, fragmentSource, geometrySource);

            s_pipelineShaders.Add(programName, newProgram);

            return newProgram;
        }

        public static PipelineShaderProgram GetPipelineShaderProgram(string vertexName, string fragmentName)
        {
            return GetPipelineShaderProgram(vertexName, fragmentName, null);
        }

        public static PipelineShaderProgram GetPipelineShaderProgram(string fragmentName)
        {
            return GetPipelineShaderProgram(DefaultShaderName, fragmentName);
        }

        public static ComputeShaderProgram GetComputeShaderProgram(string name)
        {
            if (s_computeShaders.TryGetValue(name, out ComputeShaderProgram shaderProgram))
                return shaderProgram;

            string source = s_computeSourceCache.GetItem(name);
            var program = new ComputeShaderProgram(source);

            s_computeShaders.Add(name, program);

            return program;
        }

        public static Texture2D GetTexture(string fileName)
        {
            return s_textureCache.GetItem(fileName);    
        }

        public static VirtualTexture GetVirtualTexture(string name)
        {
            VirtualTexture result = FindVirtualTexture(name);

            if (result is null)
                return s_noTexture;

            return result;
        }

        public static FontFamily GetFontFamily(string fileName)
        {
            return s_fontFamilyCache.GetItem(fileName); 
        }

        public static WaveFile GetWaveFile(string fileName)
        {
            return s_waveFileCache.GetItem(fileName);
        }

        public static Palette GetPalette(string fileName)
        {
            return s_paletteCache.GetItem(fileName);
        }

        public static int GetTileFrame(int mask)
        {
            return s_tileFrames.GetValueOrDefault(mask);
        }

        public static ControllerMapping GetControllerMapping(string name)
        {
            return s_controllerMappings.GetValueOrDefault(name);
        }

        private static void LoadVirtualTextures(Texture2D atlasTexture, XmlDocument data)
        {
            XmlElement offsets = data["Offsets"];

            foreach (XmlElement offset in offsets)
            {
                string name = offset.GetAttribute("Name");

                int x = offset.GetIntAttribute("X");
                int y = offset.GetIntAttribute("Y");
                int width = offset.GetIntAttribute("Width");
                int height = offset.GetIntAttribute("Height");

                var bounds = new Rectangle(x, y, width, height);

                s_virtualTextures.Add(name, new VirtualTexture(atlasTexture, bounds));
            }
        }

        private static VirtualTexture FindVirtualTexture(string name)
        {
            if (s_virtualTextures.TryGetValue(name, out VirtualTexture texture))
                return texture;

            return null;
        }

        private static XmlDocument LoadXML(string fileName)
        {
            string path = Path.Combine(Folder, fileName);

            return Utilities.LoadXML(path);
        }

        private static string GetLocalPathWithoutExtension(string path, string directory)
        {
            string localPath = path.Substring(directory.Length + 1);

            return Path.ChangeExtension(localPath, null);
        }
    }
}
