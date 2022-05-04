using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace PhilLibX.Media3D
{
    /// <summary>
    /// A class to hold an expandable collection of single or multi-dimensional vertex attributes such as positions, normals, etc.
    /// </summary>
    /// <typeparam name="T">Attribute type, this type must be unmanaged and not a reference type.</typeparam>
    [DebuggerDisplay("Count = {Count}")]
    public class VertexAttributeCollection<T> : ICollection<T>, IEnumerable<T> where T : unmanaged
    {
        /// <summary>
        /// Gets or Sets the number of vertices
        /// </summary>
        public int VertexCount { get; private set; }

        /// <summary>
        /// Gets the total number of entries within the collection
        /// </summary>
        public int Count => _size;

        /// <summary>
        /// Gets or Sets the dimension/values per vertex
        /// </summary>
        public int Dimension { get; private set; }

        /// <summary>
        /// Gets a value indicating if this collection is read only
        /// </summary>
        public bool IsReadOnly => false;

        /// <summary>
        /// Gets or Sets the value at the given index.
        /// </summary>
        public T this[int index]
        {
            get
            {
                if ((uint)index >= (uint)_size)
                    throw new ArgumentOutOfRangeException(nameof(index), "Index was outside the bounds of the collection");

                return _items[index];
            }
            set
            {
                if ((uint)index >= (uint)_size)
                    throw new ArgumentOutOfRangeException(nameof(index), "Index was outside the bounds of the collection");

                _items[index] = value;
                _version++;
            }
        }

        /// <summary>
        /// Gets or Sets the value at the given index.
        /// </summary>
        public T this[int vertexIndex, int itemIndex]
        {
            get
            {
                if ((uint)vertexIndex >= (uint)VertexCount)
                    throw new ArgumentOutOfRangeException(nameof(itemIndex), "Vertex Index was outside the bounds of the collection");
                if ((uint)itemIndex >= (uint)Dimension)
                    throw new ArgumentOutOfRangeException(nameof(itemIndex), "Item Index was outside the bounds of the collection");

                return this[vertexIndex * Dimension + itemIndex];
            }
            set
            {
                if ((uint)vertexIndex >= (uint)VertexCount)
                    throw new ArgumentOutOfRangeException(nameof(itemIndex), "Vertex Index was outside the bounds of the collection");
                if ((uint)itemIndex >= (uint)Dimension)
                    throw new ArgumentOutOfRangeException(nameof(itemIndex), "Item Index was outside the bounds of the collection");

                this[vertexIndex * Dimension + itemIndex] = value;
            }
        }

        /// <summary>
        /// Gets and sets the capacity of this list.  The capacity is the size of
        /// the internal array used to hold items.  When set, the internal
        /// array of the list is reallocated to the given capacity.
        /// </summary>
        public int Capacity
        {
            get => _items.Length;
            set
            {
                if (value != _items.Length)
                {
                    if (value > 0)
                    {
                        var newItems = new T[value];
                        var newCounts = new int[value];
                        if (_size > 0)
                        {
                            Array.Copy(_items, newItems, _size);
                            Array.Copy(_countPerVertex, newCounts, _size);
                        }

                        _items = newItems;
                        _countPerVertex = newCounts;
                    }
                    else
                    {
                        _items = s_emptyArray;
                        _countPerVertex = s_emptyCountPerVertex;
                    }
                }
            }
        }

        #region Private Properties
        internal T[] _items;

        internal int[] _countPerVertex;

        private int _version;

        private int _vertexIndex;

        internal int _size;

#pragma warning disable CA1825
        private static readonly T[] s_emptyArray = new T[0];
        private static readonly int[] s_emptyCountPerVertex = new int[0];
#pragma warning restore CA1825

        private const int DefaultCapacity = 4;

        private const int DefaultDimension = 1;
        #endregion

        /// <summary>
        /// Initializes a new instance of the <see cref="VertexAttributeCollection{T}"/> class
        /// </summary>
        public VertexAttributeCollection()
        {
            Dimension = DefaultDimension;
            VertexCount = 0;

            _items = s_emptyArray;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="VertexAttributeCollection{T}"/> class
        /// </summary>
        /// <param name="vertexCapacity">Initial vertex capacity</param>
        public VertexAttributeCollection(int vertexCapacity)
        {
            Dimension = DefaultDimension;
            VertexCount = 0;
            _size = 0;

            if (vertexCapacity > 0)
            {
                _items          = new T[vertexCapacity * Dimension];
                _countPerVertex = new int[vertexCapacity];
            }
            else
            {
                _items          = s_emptyArray;
                _countPerVertex = s_emptyCountPerVertex;
            }
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="VertexAttributeCollection{T}"/> class
        /// </summary>
        /// <param name="vertexCapacity">Initial vertex capacity</param>
        /// <param name="dimension">Initial dimension/values per vertex</param>
        public VertexAttributeCollection(int vertexCapacity, int dimension)
        {
            Dimension = dimension;
            VertexCount = 0;
            _size = 0;

            if (vertexCapacity > 0)
            {
                _items          = new T[vertexCapacity * Dimension];
                _countPerVertex = new int[vertexCapacity];
            }
            else
            {
                _items          = s_emptyArray;
                _countPerVertex = s_emptyCountPerVertex;
            }
        }

        public void Add()
        {
            if (_vertexIndex >= VertexCount)
                VertexCount = _vertexIndex + 1;
        }

        /// <summary>
        /// Adds an item after the previously added item to the previous vertex
        /// </summary>
        /// <param name="item">Item to add</param>
        public void Add(T item)
        {
            // Note: must get with stride
            var index = _vertexIndex * Dimension + _countPerVertex[_vertexIndex];
            _countPerVertex[_vertexIndex]++;

            // If we're outside how many items, we must expand the list
            if ((uint)index < (uint)_items.Length)
            {
                _items[index] = item;
            }
            else
            {
                Grow(index + 1);
                _items[index] = item;
            }

            if (index >= _size)
                _size = index + 1;
            if (_countPerVertex[_vertexIndex] >= Dimension)
                _vertexIndex++;
            if (_vertexIndex >= VertexCount)
                VertexCount = _vertexIndex + 1;
        }

        public void Add(T item, int vertexIndex)
        {
            if ((_countPerVertex[vertexIndex] + 1) > Dimension)
                GrowValuesPerVertex(_countPerVertex[vertexIndex] + 1);
            if (vertexIndex >= VertexCount)
                VertexCount = vertexIndex + 1;

            // Note: must get with stride
            var index = vertexIndex * Dimension + _countPerVertex[vertexIndex];
            _countPerVertex[vertexIndex]++;

            // If we're outside how many items, we must expand the list
            if ((uint)index < (uint)_items.Length)
            {
                _items[index] = item;
            }
            else
            {
                Grow(index + 1);
                _items[index] = item;
            }

            // Ensure our size is correct after add, this handles growth + dimension increase
            _size = VertexCount * Dimension;
        }

        /// <summary>
        /// If the index being set is outside the bounds of the collection, the collection is resized.
        /// </summary>
        /// <param name="item">Weight being added.</param>
        /// <param name="vertexIndex">Vertex Index</param>
        /// <param name="weightIndex">Weight Index</param>
        public void Add(T item, int vertexIndex, int weightIndex)
        {
            if (weightIndex >= Dimension)
                GrowValuesPerVertex(weightIndex);
            if (weightIndex >= _countPerVertex[vertexIndex])
                _countPerVertex[vertexIndex] = weightIndex + 1;
            if (vertexIndex >= VertexCount)
                VertexCount = vertexIndex + 1;

            // Note: must get with stride
            var index = vertexIndex * Dimension + weightIndex;

            // If we're outside how many items, we must expand the list
            if ((uint)index < (uint)_items.Length)
            {
                _items[index] = item;
            }
            else
            {
                Grow(index + 1);
                _items[index] = item;
            }

            // Ensure our size is correct after add, this handles growth + dimension increase
            _size = VertexCount * Dimension;
        }

        public void Clear()
        {
            _size = 0;
        }

        /// <summary>
        /// Checks if the provided item exists in the collection
        /// </summary>
        /// <param name="item">Item</param>
        /// <returns>True if it exists, otherwise false</returns>
        public bool Contains(T item) => _size != 0 && Array.IndexOf(_items, item) != -1;

        public void CopyTo(T[] array, int arrayIndex)
        {
            throw new NotImplementedException();
        }


        public int FindIndex(Predicate<T> match) => FindIndex(0, VertexCount * Dimension, match);

        public int FindIndex(int vertexIndex, Predicate<T> match) => FindIndex(vertexIndex * Dimension, Dimension, match);

        public int FindIndex(int startIndex, int count, Predicate<T> match)
        {
            int endIndex = startIndex + count;

            for (int i = startIndex; i < endIndex; i++)
            {
                if (match(_items[i]))
                    return i;
            }

            return -1;
        }

        public IEnumerator<T> GetEnumerator() => new Enumerator(this);

        IEnumerator IEnumerable.GetEnumerator() => new Enumerator(this);

        public bool Remove(T item)
        {
            throw new NotImplementedException();
        }

        public T[] GetArray()
        {
            return _items;
        }

        #region Private Methods
        private void Grow(int capacity)
        {
            Debug.Assert(_items.Length < capacity);

            int nCapacity = _items.Length == 0 ? VertexCount * Dimension * DefaultCapacity : 2 * _items.Length;

            if (nCapacity < capacity) nCapacity = capacity;

            Capacity = nCapacity;
        }

        private void GrowValuesPerVertex(int newValuesPerVertex)
        {
            int nValuesPerVertex = _items.Length == 0 ? VertexCount * Dimension * DefaultCapacity : 2 * _items.Length;

            var newItems = new T[newValuesPerVertex * VertexCount];

            for (int v = 0; v < VertexCount; v++)
            {
                var oldOffset = v * Dimension;
                var newOffset = v * newValuesPerVertex;

                for (int w = 0; w < newValuesPerVertex && w < Dimension; w++)
                {
                    newItems[newOffset + w] = _items[oldOffset + w];
                }
            }
        }
        #endregion

        /// <summary>
        /// A struct to hold a <see cref="VertexAttributeCollection{T}"/> enumerator
        /// </summary>
        public struct Enumerator : IEnumerator<T>, IEnumerator
        {
            #region Internal/Private
            private readonly VertexAttributeCollection<T> _collection;
            private int _index;
            private readonly int _version;
            private T _current;

            internal Enumerator(VertexAttributeCollection<T> collection)
            {
                _collection = collection;
                _index = 0;
                _version = collection._version;
                _current = default;
            }
            #endregion

            /// <summary>
            /// Disposes of the current enumerator
            /// </summary>
            public void Dispose()
            {
            }

            /// <summary>
            /// Moves to the next item
            /// </summary>
            public bool MoveNext()
            {
                var localList = _collection;

                if (_version == localList._version && ((uint)_index < (uint)localList._size))
                {
                    _current = localList._items[_index];
                    _index++;
                    return true;
                }
                return MoveNextRare();
            }

            /// <summary>
            /// Moves to the next item
            /// </summary>
            private bool MoveNextRare()
            {
                if (_version != _collection._version)
                    throw new InvalidOperationException("Collection was modified; enumeration operation may not execute.");

                _index = _collection._size + 1;
                _current = default;
                return false;
            }

            /// <summary>
            /// Gets the current item
            /// </summary>
            public T Current => _current;

            /// <summary>
            /// Gets the current item
            /// </summary>
            object IEnumerator.Current
            {
                get
                {
                    if (_version != _collection._version)
                        throw new InvalidOperationException("Collection was modified; enumeration operation may not execute.");
                    return Current;
                }
            }

            /// <summary>
            /// Resets the enumerator
            /// </summary>
            void IEnumerator.Reset()
            {
                if (_version != _collection._version)
                    throw new InvalidOperationException("Collection was modified; enumeration operation may not execute.");
                _index = 0;
                _current = default;
            }
        }
    }
}
