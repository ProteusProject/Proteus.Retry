using System;
using System.Collections;
using System.Collections.Generic;

namespace Proteus.Retry
{
    public class ExceptionList : IList<Type>
    {
        private readonly IList<Type> _inner = new List<Type>();

        private void ThrowIfTypeIsNotException(Type type)
        {
            if (type == typeof (Exception) || type.IsSubclassOf(typeof (Exception)))
                return;

            throw new ArgumentException(string.Format("{0} can only contain types derived from System.Exception.", this.GetType().FullName));
        }

        #region IList<Type> members
        
        public IEnumerator<Type> GetEnumerator()
        {
            return _inner.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return ((IEnumerable)_inner).GetEnumerator();
        }

        public void Add(Type item)
        {
            ThrowIfTypeIsNotException(item);
            _inner.Add(item);
        }

        public void Clear()
        {
            _inner.Clear();
        }

        public bool Contains(Type item)
        {
            return _inner.Contains(item);
        }

        public void CopyTo(Type[] array, int arrayIndex)
        {
            _inner.CopyTo(array, arrayIndex);
        }

        public bool Remove(Type item)
        {
            return _inner.Remove(item);
        }

        public int Count
        {
            get { return _inner.Count; }
        }

        public bool IsReadOnly
        {
            get { return _inner.IsReadOnly; }
        }

        public int IndexOf(Type item)
        {
            return _inner.IndexOf(item);
        }

        public void Insert(int index, Type item)
        {
            ThrowIfTypeIsNotException(item);
            _inner.Insert(index, item);
        }

        public void RemoveAt(int index)
        {
            _inner.RemoveAt(index);
        }

        public Type this[int index]
        {
            get { return _inner[index]; }
            set
            {
                ThrowIfTypeIsNotException(value);
                _inner[index] = value;
            }
        }
        
        #endregion
    }
}
