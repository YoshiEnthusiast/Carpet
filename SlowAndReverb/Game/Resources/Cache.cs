using System;
using System.Collections.Generic;
using System.IO;

namespace SlowAndReverb
{
    public abstract class Cache<T>
    {
        public const string ContentFolder = "Content";

        private readonly Dictionary<string, T> _items = new Dictionary<string, T>();
        private readonly string _mainDirectory;

        public Cache(string mainDirectory, bool load)
        {
            if (mainDirectory is not null)
            {
                _mainDirectory = Path.Combine(ContentFolder, mainDirectory);

                if (load)
                    LoadFromDirectory(_mainDirectory);
            }
        }

        public Cache() : this(null, false)
        {

        }

        public string MainDirectory => _mainDirectory;
        public string Extension { get; protected set; }

        public T GetItem(string fileName)
        {
            if (_items.TryGetValue(fileName, out T item))
                return item;

            string path = GetPath(fileName);

            return AddItem(path);
        }

        public void LoadFromDirectory(string directory)
        {
            foreach (string path in Directory.GetFiles(directory))
                if (Path.GetExtension(path) == Extension)
                    AddItem(path);

            foreach (string subDirectory in Directory.GetDirectories(directory))
                LoadFromDirectory(subDirectory);
        }

        public void Clear()
        {
            _items.Clear();
        }

        public string GetPath(string fileName)
        {
            if (!Path.HasExtension(fileName))
            {
                fileName += Extension;
            }
            else
            {
                string extension = Path.GetExtension(fileName);

                if (extension != Extension)
                    throw new Exception($"This is an invalid file extension for this type of content({extension}). Expected extension: {Extension}");
            }

            if (File.Exists(fileName) || _mainDirectory is null)
                return fileName;

            return Path.Combine(_mainDirectory, fileName);
        }

        protected abstract T CreateItem(string path);

        private T AddItem(string path)
        {
            T item = CreateItem(path);

            _items.Add(path, item);

            return item;    
        }
    }
}
