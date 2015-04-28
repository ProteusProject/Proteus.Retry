﻿using System;
using System.Collections;
using System.Collections.Generic;

namespace Proteus.Retry
{
    /// <summary>
    /// Class ConstrainedTypesList.
    /// </summary>
    /// <typeparam name="TConstraint">The type to which the list is constrained.</typeparam>
    public class ConstrainedTypesList<TConstraint> : IList<Type>
    {
        /// <summary>
        /// The _inner
        /// </summary>
        private readonly IList<Type> _inner = new List<Type>();

        /// <summary>
        /// Throws the type of if type is not constrained.
        /// </summary>
        /// <param name="type">The type.</param>
        /// <exception cref="System.ArgumentException"></exception>
        private void ThrowIfTypeIsNotConstrainedType(Type type)
        {
            if (type == typeof(TConstraint) || type.IsSubclassOf(typeof(TConstraint)))
                return;

            throw new ArgumentException(string.Format("This instance of Proteus.Retry.ConstrainedTypesList<Type> can only accept {0} or types derived from {0}.", typeof(TConstraint).FullName));
        }

        #region IList<Type> members

        /// <summary>
        /// Returns an enumerator that iterates through the collection.
        /// </summary>
        /// <returns>A <see cref="T:System.Collections.Generic.IEnumerator`1" /> that can be used to iterate through the collection.</returns>
        public IEnumerator<Type> GetEnumerator()
        {
            return _inner.GetEnumerator();
        }

        /// <summary>
        /// Returns an enumerator that iterates through a collection.
        /// </summary>
        /// <returns>An <see cref="T:System.Collections.IEnumerator" /> object that can be used to iterate through the collection.</returns>
        IEnumerator IEnumerable.GetEnumerator()
        {
            return ((IEnumerable)_inner).GetEnumerator();
        }

        /// <summary>
        /// Adds an item to the <see cref="T:System.Collections.Generic.ICollection`1" />.
        /// </summary>
        /// <param name="item">The object to add to the <see cref="T:System.Collections.Generic.ICollection`1" />.</param>
        public void Add(Type item)
        {
            ThrowIfTypeIsNotConstrainedType(item);
            _inner.Add(item);
        }

        /// <summary>
        /// Removes all items from the <see cref="T:System.Collections.Generic.ICollection`1" />.
        /// </summary>
        public void Clear()
        {
            _inner.Clear();
        }

        /// <summary>
        /// Determines whether the <see cref="T:System.Collections.Generic.ICollection`1" /> contains a specific value.
        /// </summary>
        /// <param name="item">The object to locate in the <see cref="T:System.Collections.Generic.ICollection`1" />.</param>
        /// <returns>true if <paramref name="item" /> is found in the <see cref="T:System.Collections.Generic.ICollection`1" />; otherwise, false.</returns>
        public bool Contains(Type item)
        {
            return _inner.Contains(item);
        }

        /// <summary>
        /// Copies the contents to a provided array.
        /// </summary>
        /// <param name="array">The array.</param>
        /// <param name="arrayIndex">Index of the array.</param>
        public void CopyTo(Type[] array, int arrayIndex)
        {
            _inner.CopyTo(array, arrayIndex);
        }

        /// <summary>
        /// Removes the first occurrence of a specific object from the <see cref="T:System.Collections.Generic.ICollection`1" />.
        /// </summary>
        /// <param name="item">The object to remove from the <see cref="T:System.Collections.Generic.ICollection`1" />.</param>
        /// <returns>true if <paramref name="item" /> was successfully removed from the <see cref="T:System.Collections.Generic.ICollection`1" />; otherwise, false. This method also returns false if <paramref name="item" /> is not found in the original <see cref="T:System.Collections.Generic.ICollection`1" />.</returns>
        public bool Remove(Type item)
        {
            return _inner.Remove(item);
        }

        /// <summary>
        /// Gets the number of elements contained in the <see cref="T:System.Collections.Generic.ICollection`1" />.
        /// </summary>
        /// <value>The count.</value>
        public int Count
        {
            get { return _inner.Count; }
        }

        /// <summary>
        /// Gets a value indicating whether the <see cref="T:System.Collections.Generic.ICollection`1" /> is read-only.
        /// </summary>
        /// <value><c>true</c> if this instance is read only; otherwise, <c>false</c>.</value>
        public bool IsReadOnly
        {
            get { return _inner.IsReadOnly; }
        }

        /// <summary>
        /// Determines the index of a specific item in the <see cref="T:System.Collections.Generic.IList`1" />.
        /// </summary>
        /// <param name="item">The object to locate in the <see cref="T:System.Collections.Generic.IList`1" />.</param>
        /// <returns>The index of <paramref name="item" /> if found in the list; otherwise, -1.</returns>
        public int IndexOf(Type item)
        {
            return _inner.IndexOf(item);
        }

        /// <summary>
        /// Inserts an item to the <see cref="T:System.Collections.Generic.IList`1" /> at the specified index.
        /// </summary>
        /// <param name="index">The zero-based index at which <paramref name="item" /> should be inserted.</param>
        /// <param name="item">The object to insert into the <see cref="T:System.Collections.Generic.IList`1" />.</param>
        public void Insert(int index, Type item)
        {
            ThrowIfTypeIsNotConstrainedType(item);
            _inner.Insert(index, item);
        }

        /// <summary>
        /// Removes the <see cref="T:System.Collections.Generic.IList`1" /> item at the specified index.
        /// </summary>
        /// <param name="index">The zero-based index of the item to remove.</param>
        public void RemoveAt(int index)
        {
            _inner.RemoveAt(index);
        }

        /// <summary>
        /// Gets or sets the element at the specified index.
        /// </summary>
        /// <param name="index">The index.</param>
        /// <returns>Type.</returns>
        public Type this[int index]
        {
            get { return _inner[index]; }
            set
            {
                ThrowIfTypeIsNotConstrainedType(value);
                _inner[index] = value;
            }
        }

        #endregion
    }
}
