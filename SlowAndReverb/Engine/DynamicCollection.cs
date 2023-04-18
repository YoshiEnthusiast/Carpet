﻿using System.Collections;
using System.Collections.Generic;

namespace SlowAndReverb
{
    public class DynamicCollection<T> : IEnumerable<T>
    {
        private readonly HashSet<T> _items = new HashSet<T>();

        private readonly HashSet<T> _itemsToAdd = new HashSet<T>();
        private readonly HashSet<T> _itemsToRemove = new HashSet<T>();

        public void Update()
        {
            foreach (T itemToRemove in _itemsToRemove)
            {
                if (!_items.Remove(itemToRemove))
                    continue;

                OnItemRemoved(itemToRemove);
            }

            OnUpdate();

            foreach (T itemToAdd in _itemsToAdd)
            {
                if (!_items.Add(itemToAdd))
                    continue;

                OnItemAdded(itemToAdd);
            }

            _itemsToAdd.Clear();
            _itemsToRemove.Clear();
        }

        public void Add(T item)
        {
            _itemsToAdd.Add(item);
        }

        public void Remove(T item)
        {
            _itemsToRemove.Add(item);
        }

        public virtual void Clear()
        {
            _items.Clear();
            _itemsToAdd.Clear();
            _itemsToRemove.Clear();
        }

        public IEnumerator<T> GetEnumerator()
        {
            return _items.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        protected virtual void OnUpdate()
        {

        }

        protected virtual void OnItemAdded(T item)
        {

        }

        protected virtual void OnItemRemoved(T item)
        {

        }
    }
}