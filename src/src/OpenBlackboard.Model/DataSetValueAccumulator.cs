using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace OpenBlackboard.Model
{
    sealed class DataSetValueAccumulator
    {
        public bool IsEmpty => _items.Count == 0;

        public IEnumerable<object> this[ValueDescriptor descriptor]
        {
            get
            {
                Debug.Assert(descriptor != null);
                Debug.Assert(_items != null);

                List<object> values;
                if (_items.TryGetValue(descriptor, out values))
                    return values;

                return Enumerable.Empty<object>();
            }
        }

        public void Add(DataSetValue value)
        {
            Debug.Assert(value != null);
            Debug.Assert(_items != null);

            List<object> list;
            if (!_items.TryGetValue(value.Descriptor, out list))
            {
                list = new List<object>();
                _items.Add(value.Descriptor, list);
            }

            list.Add(value.Value);
        }

        public void AddRange(IEnumerable<DataSetValue> values)
        {
            Debug.Assert(values != null);

            foreach (var value in values)
                Add(value);
        }

        public void Clear()
        {
            Debug.Assert(_items != null);

            _items.Clear();
        }

        private readonly Dictionary<ValueDescriptor, List<object>> _items = new Dictionary<ValueDescriptor, List<object>>();
    }
}
