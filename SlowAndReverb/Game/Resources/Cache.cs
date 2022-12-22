using System;
using System.Collections.Generic;
using System.IO;

namespace SlowAndReverb
{
    public abstract class Cache<T>
    {
        public const string ContentFolder = "Content";

        private readonly Dictionary<string, T> _items = new Dictionary<string, T>();

        private readonly string _extension;
        private readonly string _mainDirectory;

        public Cache(string extension, string mainDirectory, bool load)
        {
            _extension = extension;

            if (mainDirectory is not null)
            {
                _mainDirectory = Path.Combine(ContentFolder, mainDirectory);

                if (load)
                    LoadFromDirectory(_mainDirectory);
            }
        }

        public string MainDirectory => _mainDirectory;
        public string Extension => _extension;  

        public T GetItem(string fileName)
        {
            string path = GetPath(fileName);

            if (_items.TryGetValue(path, out T item))
                return item;

            return AddItem(path);
        }

        public void LoadFromDirectory(string directory)
        {
            foreach (string path in Directory.GetFiles(directory))
                if (Path.GetExtension(path) == _extension)
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
                fileName += _extension;
            }
            else
            {
                string extension = Path.GetExtension(fileName);

                if (extension != _extension)
                    throw new Exception($"This is an invalid file extension for this type of content({extension}). Expected extension: {_extension}");
            }

            if (File.Exists(fileName) || _mainDirectory is null)
                return fileName;

            return Path.Combine(_mainDirectory, fileName);
        }

        public IEnumerable<CachedItem<T>> GetAllValues()
        {
            foreach (string key in _items.Keys)
                yield return new CachedItem<T>(_items[key], key);
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
