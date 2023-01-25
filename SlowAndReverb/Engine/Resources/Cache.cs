using System;
using System.Collections.Generic;
using System.IO;

namespace SlowAndReverb
{
    public abstract class Cache<T>
    {
        private readonly Dictionary<string, T> _items = new Dictionary<string, T>();

        public Cache(string extension, string mainDirectory, bool load)
        {
            Extension = extension;

            if (mainDirectory is not null)
            {
                MainDirectory = Path.Combine(Content.Folder, mainDirectory);

                if (load)
                    LoadFromDirectory(MainDirectory);
            }
        }

        public string MainDirectory { get; private init; }
        public string Extension { get; private init; }

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
                fileName = AddExtention(fileName);
            }
            else
            {
                string extension = Path.GetExtension(fileName);

                if (!IsValidExtension(extension))                                                                         // Remove this??????
                    throw new Exception($"This is an invalid file extension for this type of content({extension}). Expected extension: {Extension}");
            }

            if (File.Exists(fileName) || MainDirectory is null)
                return fileName;

            return Path.Combine(MainDirectory, fileName);
        }

        public IEnumerable<CachedItem<T>> GetAllValues()
        {
            foreach (string key in _items.Keys)
                yield return new CachedItem<T>(_items[key], key);
        }

        protected abstract T CreateItem(string path);

        protected virtual string AddExtention(string fileName)
        {
            return fileName + Extension;
        }

        protected virtual bool IsValidExtension(string extension)
        {
            return extension == Extension; 
        }

        private T AddItem(string path)
        {
            T item = CreateItem(path);

            _items.Add(path, item);

            return item;    
        }
    }
}
