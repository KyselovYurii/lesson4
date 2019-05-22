using System;
using System.Collections.Generic;

namespace lesson4
{
    public class Transaction<TK, TV> : IDisposable
    {
        private readonly TransactionalWrapper<TK, TV> _parentWrapper;
        private Dictionary<TK, TV> _pairsToAdd = new Dictionary<TK, TV>();
        private HashSet<TK> _pairsToRemove = new HashSet<TK>();

        internal Transaction(TransactionalWrapper<TK, TV> parentWrapper)
        {
            _parentWrapper = parentWrapper ?? throw new ArgumentNullException(nameof(parentWrapper));
        }

        public bool IsDisposed { get; private set; }

        public int Count => _parentWrapper.Count + _pairsToAdd.Count - _pairsToRemove.Count;

        public void Add(TK key, TV value)
        {
            CheckIfCommited();

            if (_pairsToRemove.Contains(key))
            {
                _pairsToRemove.Remove(key);
            }
            else if(_parentWrapper.ContainsKey(key))
            {
                throw new ArgumentException("An element with the same key already exists in the collection.");
            }
            else
            {
                _pairsToAdd.Add(key, value);
            }
        }

        public TV Get(TK key)
        {
            CheckIfCommited();
            
            if (_pairsToRemove.Contains(key))
            {
                throw new KeyNotFoundException("The key does not exist in the collection.");
            }

            if (_pairsToAdd.TryGetValue(key, out TV deltaValue))
            {
                return deltaValue;
            }

            return _parentWrapper[key];
        }

        public bool TryGet(TK key, out TV value)
        {
            CheckIfCommited();

            value = default(TV);

            if (_pairsToRemove.Contains(key))
            {
                return false;
            }

            if (_parentWrapper.TryGetValue(key, out TV outValue1))
            {
                value = outValue1;
                return true;
            }

            if (_pairsToAdd.TryGetValue(key, out TV outValue2))
            {
                value = outValue2;
                return true;
            }

            return false;
        }

        public void Remove(TK key)
        {
            CheckIfCommited();

            if (_parentWrapper.TryGetValue(key, out TV value))
            {
                _pairsToRemove.Add(key);
            }
            else if (_pairsToAdd.TryGetValue(key, out _))
            {
                _pairsToAdd.Remove(key);
            }
        }

        internal void Commit()
        {
            CheckIfCommited();

            foreach (var pairToRemove in _pairsToRemove)
            {
                _parentWrapper.Remove(pairToRemove);
            }

            foreach (var pairToAdd in _pairsToAdd)
            {
                _parentWrapper.Add(pairToAdd.Key, pairToAdd.Value);
            }
        }

        internal void RollBack()
        {
            CheckIfCommited();

            _pairsToRemove.Clear();
            _pairsToAdd.Clear();
        }

        private void CheckIfCommited()
        {
            if(IsDisposed)
            {
                throw new InvalidOperationException();
            }
        }

        public void Dispose()
        {
            Commit();
        }
    }
}