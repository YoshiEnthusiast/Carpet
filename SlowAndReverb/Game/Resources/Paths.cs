using System.Collections;
using System.Collections.Generic;
using System.IO;

namespace SlowAndReverb
{
    public class Paths
    {
        private const string ContentDirectory = "Content";

        public static readonly string ShadersDirectory = ConcatenatePaths(ContentDirectory, "Shaders");

        public static readonly ContentStorage TexturesStorage = new ContentStorage(ConcatenatePaths(ContentDirectory, "Textures"), ".png");
        public static readonly ContentStorage VertexShadersStorage = new ContentStorage(ConcatenatePaths(ShadersDirectory, "Vertex"), ".vert");
        public static readonly ContentStorage FragmentShadersStorage = new ContentStorage(ConcatenatePaths(ShadersDirectory, "Fragment"), ".frag");
        public static readonly ContentStorage GeometryShadersStorage = new ContentStorage(ConcatenatePaths(ShadersDirectory, "Geometry"), ".geom");
        public static readonly ContentStorage FontsStorage = new ContentStorage(ConcatenatePaths(ContentDirectory, "Fonts"), ".png");
        public static readonly ContentStorage SoundsStorage = new ContentStorage(ConcatenatePaths(ContentDirectory, "SFX"), ".wav");

        public static readonly string FontsDataPath = ConcatenatePaths(ContentDirectory, "fonts.xml");

        public static string GetTexturePath(string fileName)
        {
            return GetPath(fileName, TexturesStorage);
        }

        public static string GetWaveFilePath(string fileName)
        {
            return GetPath(fileName, SoundsStorage);
        }

        public static string GetVertexShaderPath(string fileName)
        {
            return GetPath(fileName, VertexShadersStorage);
        }

        public static string GetFragmentShaderPath(string fileName)
        {
            return GetPath(fileName, FragmentShadersStorage);
        }

        public static string GetGeometryShaderPath(string fileName)
        {
            return GetPath(fileName, GeometryShadersStorage);
        }

        public static string GetFontPath(string fileName)
        {
            return GetPath(fileName, FontsStorage);
        }

        public static string ConcatenatePaths(string path, string other)
        {
            return @$"{path}\{other}";
        }

        private static string GetPath(string fileName, ContentStorage storage)
        {
            string fileNameWithExtension = AddExtension(fileName, storage.Extension);
            string completedPath = ConcatenatePaths(storage.Directory, fileNameWithExtension);

            if (File.Exists(completedPath))
                return completedPath;

            return fileNameWithExtension;
        }

        private static string AddExtension(string fileName, string extension)
        {
            if (!Path.HasExtension(fileName))
                return fileName + extension;

            return fileName;
        }
    }
}
