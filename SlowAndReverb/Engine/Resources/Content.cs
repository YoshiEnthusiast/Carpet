﻿using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Xml;

namespace SlowAndReverb
{
    public static class Content
    {
        public const string EncodedFileExtsntion = ".rsc";
        public const string DefaultShaderName = "default";

        private const string AtlasFileName = "atlas.rsc";
        private const string AtlasDataFileName = "atlas.xml";

        private static TextureCache s_textureCache;
        private static FontFamilyCache s_fontFamilyCache;
        private static WaveFileCache s_waveFileCache;

        private static ShaderSourceCache s_vertexSourceCache;
        private static ShaderSourceCache s_fragmentSourceCache;
        private static ShaderSourceCache s_geometrySourceCache;

        private static VirtualTexture s_noTexture;

        private static readonly Dictionary<string, ShaderProgram> s_shaders = new Dictionary<string, ShaderProgram>();
        private static readonly Dictionary<string, VirtualTexture> s_virtualTextures = new Dictionary<string, VirtualTexture>();

        internal static void LoadGraphics(TextureLoadMode mode)
        {
            s_vertexSourceCache = new ShaderSourceCache("Shaders/Vertex", ".vert", true);
            s_fragmentSourceCache = new ShaderSourceCache("Shaders/Fragment", ".frag", true);
            s_geometrySourceCache = new ShaderSourceCache("Shaders/Geometry", ".geom", true);

            s_textureCache = new TextureCache("Textures", true);

            string texturesDirectory = s_textureCache.MainDirectory;

            string atlasFileName = Path.Combine(texturesDirectory, AtlasFileName);
            string atlasDataFileName = Path.Combine(texturesDirectory, AtlasDataFileName);

            if (mode >= TextureLoadMode.CreateAtlas)
            {
                var atlas = new Atlas(OpenGL.MaxTextureSize);

                IEnumerable<CachedItem<Texture>> items = s_textureCache.GetAllValues();
                int texturesDirectoryLength = texturesDirectory.Length;

                foreach (CachedItem<Texture> item in items)
                {
                    string path = item.Path;
                    string localPath = path.Substring(texturesDirectoryLength + 1);

                    if (localPath == AtlasFileName)
                        continue;

                    string name = Path.ChangeExtension(localPath, null);

                    atlas.Add(item.Value, name);
                }

                atlas.Build(5);

                foreach (CachedItem<Texture> item in items)
                {
                    Texture texture = item.Value;

                    texture.Bind();
                    texture.Delete();
                }

                s_textureCache.Clear();

                Texture atlasTexture = atlas.Texture;
                XmlDocument atlasData = atlas.Data;

                if (mode == TextureLoadMode.SaveAtlas)
                {
                    atlasTexture.SaveAsPng(atlasFileName);
                    atlasData.Save(atlasDataFileName);
                }

                LoadVirtualTextures(atlasTexture, atlasData);
            }
            else
            {
                Texture atlasTexture = GetTexture(atlasFileName);
                XmlDocument atlasData = Utilities.LoadXML(atlasDataFileName);

                LoadVirtualTextures(atlasTexture, atlasData);
            }

            s_fontFamilyCache = new FontFamilyCache("Fonts", true);
            s_noTexture = FindVirtualTexture("noTexture");

            s_shaders.Clear();
        }

        internal static void Load()
        {
            s_waveFileCache = new WaveFileCache("SFX", true);

            // Levels
            // Save data????
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

        private static void LoadVirtualTextures(Texture atlasTexture, XmlDocument data)
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
    }
}