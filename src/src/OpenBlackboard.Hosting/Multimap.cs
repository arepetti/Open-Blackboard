﻿using System;
using System.Collections.Generic;
using System.Linq;

namespace OpenBlackboard.Hosting
{
    sealed class Multimap<TKey, TValue>
    {
        public IEnumerable<TKey> Keys => _items.Keys;

        public void Add(TKey key, TValue value)
        {
            List<TValue> list;
            if (!_items.TryGetValue(key, out list))
            {
                list = new List<TValue>();
                _items.Add(key, list);
            }

            list.Add(value);
        }

        public bool Remove(TKey key, TValue value)
        {
            List<TValue> list;
            if (!_items.TryGetValue(key, out list))
                return false;

            if (!list.Remove(value))
                return false;

            if (list.Count == 0)
                _items.Remove(key);

            return true;
        }

        public IEnumerable<TValue> GetAll(TKey key)
        {
            List<TValue> list;
            if (_items.TryGetValue(key, out list))
                return list;

            return Enumerable.Empty<TValue>();
        }

        public bool Contains(TKey key)
        { 
            return _items.ContainsKey(key);
        }

        private readonly Dictionary<TKey, List<TValue>> _items = new Dictionary<TKey, List<TValue>>();
    }
}
