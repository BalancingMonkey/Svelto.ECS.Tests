﻿namespace Svelto.ECS
{
    public readonly ref struct EntityFilterIterator
    {
        internal EntityFilterIterator(EntityFilterCollection filter)
        {
            _filter = filter;
        }

        public Iterator GetEnumerator() => new Iterator(_filter);

        readonly EntityFilterCollection _filter;

        public ref struct Iterator
        {
            internal Iterator(EntityFilterCollection filter)
            {
                _filter = filter;
                _indexGroup = -1;
                _current = default;
            }

            public bool MoveNext()
            {
                while (++_indexGroup < _filter.groupCount)
                {
                    _current  = _filter.GetGroup(_indexGroup);

                    if (_current.count > 0) break;
                }

                return _indexGroup < _filter.groupCount;
            }

            public void Reset()
            {
                _indexGroup = -1;
            }

            public RefCurrent Current => new RefCurrent(_current);

            int                    _indexGroup;
            EntityFilterCollection _filter;
            EntityFilterCollection.GroupFilters  _current;
        }

        public readonly ref struct RefCurrent
        {
            internal RefCurrent(EntityFilterCollection.GroupFilters filter)
            {
                _filter = filter;
            }

            public void Deconstruct(out EntityFilterIndices indices, out ExclusiveGroupStruct group)
            {
                indices = _filter.indices;
                group = _filter.group;
            }

            readonly EntityFilterCollection.GroupFilters _filter;
        }
    }
}