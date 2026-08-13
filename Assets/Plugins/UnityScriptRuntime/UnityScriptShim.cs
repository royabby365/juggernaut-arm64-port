// UnityScript.Lang shim — replaces the vendored UnityScript.Lang.dll (old
// mscorlib 2.0.5.0 base, broke the Unity API Updater pass).
// Minimal surface used by the decompiled UnityScript_Converted files.
using System;
using System.Collections;
using System.Collections.Generic;

namespace UnityScript.Lang
{
    /// <summary>UnityScript-style dynamic Array (sendmessages, lock_data, ...).</summary>
    public class Array
    {
        private readonly List<object> _items = new List<object>();

        public Array()
        {
        }

        public Array(IEnumerable<object> items)
        {
            _items.AddRange(items);
        }

        public static implicit operator Array(object[] items)
        {
            var a = new Array();
            if (items != null)
            {
                a._items.AddRange(items);
            }
            return a;
        }

        public static implicit operator object[](Array a)
        {
            return a == null ? new object[0] : a._items.ToArray();
        }

        public int length => _items.Count;

        public object this[int index]
        {
            get => _items[index];
            set => _items[index] = value;
        }

        public void Add(object item)
        {
            _items.Add(item);
        }

        /// <summary>Removes and discards the first element (UnityScript shift()).</summary>
        public void shift()
        {
            if (_items.Count > 0)
            {
                _items.RemoveAt(0);
            }
        }

        /// <summary>Adds an item to the end (UnityScript push()).</summary>
        public int push(object item)
        {
            _items.Add(item);
            return _items.Count;
        }
    }

    /// <summary>Length helpers for strings / arrays / collections.</summary>
    public static class Extensions
    {
        public static int get_length(object value)
        {
            if (value == null)
            {
                return 0;
            }
            if (value is string)
            {
                return ((string)value).Length;
            }
            if (value is Array)
            {
                return ((Array)value).length;
            }
            if (value is ICollection)
            {
                return ((ICollection)value).Count;
            }
            if (value is System.Array)
            {
                return ((System.Array)value).Length;
            }
            return 0;
        }
    }

    /// <summary>Enumerator helpers (UnityScript runtime).</summary>
    public static class UnityRuntimeServices
    {
        public static IEnumerator GetEnumerator(object value)
        {
            if (value == null)
            {
                return null;
            }
            if (value is IEnumerator)
            {
                return (IEnumerator)value;
            }
            if (value is IEnumerable)
            {
                return ((IEnumerable)value).GetEnumerator();
            }
            // UnityScript wraps a single value in a one-element enumeration.
            return new object[] { value }.GetEnumerator();
        }

        public static bool Update(ref IEnumerator enumerator, object target)
        {
            IEnumerator newEnumerator = GetEnumerator(target);
            if (newEnumerator != enumerator)
            {
                enumerator = newEnumerator;
                return true;
            }
            return false;
        }
    }
}