using System;
using System.Collections.Generic;

namespace TJNK.Farwander.Core
{
    /// <summary>Type-driven registry of factories. No implicit caching; factories decide lifecycle.</summary>
    public sealed class QueryRegistry
    {
        private readonly Dictionary<Type, Func<object>> _factories = new Dictionary<Type, Func<object>>();

        public void Register<T>(Func<T> factory)
        {
            if (factory == null) throw new ArgumentNullException("factory");
            _factories[typeof(T)] = () => factory();
        }

        public T Get<T>() where T : class
        {
            Func<object> f; return _factories.TryGetValue(typeof(T), out f) ? f() as T : null;
        }
    }
}