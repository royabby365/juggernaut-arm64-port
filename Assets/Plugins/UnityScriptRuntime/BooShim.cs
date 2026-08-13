// Boo.Lang shim — replaces the vendored Boo.Lang.dll (which referenced old
// mscorlib 2.0.5.0 and broke the Unity API Updater pass).
// Only the API surface the decompiled UnityScript_Converted files use.
using System;
using System.Collections;
using System.Collections.Generic;

namespace Boo.Lang
{
    /// <summary>Factory class for the decompiled coroutine state machines.</summary>
    public abstract class GenericGenerator<T> : IEnumerator<T>
    {
        private IEnumerator<T> _enumerator;

        public abstract IEnumerator<T> GetEnumerator();

        private IEnumerator<T> Enumerator => _enumerator ?? (_enumerator = GetEnumerator());

        public T Current => Enumerator.Current;

        object IEnumerator.Current => Current;

        public bool MoveNext() => Enumerator.MoveNext();

        public void Reset() => Enumerator.Reset();

        public void Dispose()
        {
        }
    }

    /// <summary>Enumerator base for the decompiled coroutine state machines.</summary>
    public abstract class GenericGeneratorEnumerator<T> : IEnumerator<T>, IEnumerator
    {
        protected int _state;

        public GenericGeneratorEnumerator()
        {
            _state = 0;
        }

        public virtual T Current { get; protected set; }

        object IEnumerator.Current => Current;

        public abstract bool MoveNext();

        public virtual void Reset()
        {
            _state = 0;
        }

        public virtual void Dispose()
        {
        }

        protected bool Yield(int nextState, T value)
        {
            _state = nextState;
            Current = value;
            return true;
        }

        protected bool YieldDefault(int nextState)
        {
            _state = nextState;
            return true;
        }
    }

    /// <summary>Boo's Hash type - a string-keyed dictionary with dynamic values.</summary>
    /// UnityScript/Boo uses Hash as Dictionary<string, object> with some syntactic sugar.
    [Serializable]
    public class Hash : Dictionary<string, object>, IEnumerable
    {
        public Hash() : base() { }

        public Hash(IDictionary<string, object> dictionary) : base(dictionary) { }

        public Hash(params object[] args) : base()
        {
            if (args.Length % 2 != 0)
            {
                throw new ArgumentException("Hash initializer requires an even number of arguments (key, value pairs).");
            }
            for (int i = 0; i < args.Length; i += 2)
            {
                string key = args[i] as string;
                if (key == null)
                {
                    throw new ArgumentException("Hash keys must be strings.");
                }
                this[key] = args[i + 1];
            }
        }

        // Allow indexer access with object key (for dynamic dispatch)
        new public object this[string key]
        {
            get => base.TryGetValue(key, out var value) ? value : null;
            set => base[key] = value;
        }

        // Boo.Lang.Runtime.RuntimeServices.GetEnumerator support
        public new IEnumerator GetEnumerator() => base.GetEnumerator();
    }

    namespace Runtime
    {
        /// <summary>Boo's dynamic runtime helpers, minimal compatible surface.</summary>
        public static class RuntimeServices
        {
            public static bool ToBool(object value)
            {
                if (value == null)
                {
                    return false;
                }
                if (value is bool)
                {
                    return (bool)value;
                }
                if (value is string)
                {
                    return ((string)value).Length > 0;
                }
                if (value is IConvertible)
                {
                    try
                    {
                        return Convert.ToDouble(value) != 0.0;
                    }
                    catch
                    {
                        return true;
                    }
                }
                return true;
            }

            public static object Coerce(object value, Type targetType)
            {
                if (value == null || targetType == null)
                {
                    return value;
                }
                if (targetType.IsInstanceOfType(value))
                {
                    return value;
                }
                try
                {
                    return Convert.ChangeType(value, targetType);
                }
                catch
                {
                    return value;
                }
            }

            public static bool EqualityOperator(object lhs, object rhs)
            {
                if (lhs == rhs)
                {
                    return true;
                }
                if (lhs == null || rhs == null)
                {
                    return false;
                }
                return lhs.Equals(rhs);
            }
        }
    }
}