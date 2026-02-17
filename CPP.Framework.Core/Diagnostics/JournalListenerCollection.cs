using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using CPP.Framework.Threading;

namespace CPP.Framework.Diagnostics
{
    /// <summary>
    /// Represents a collection of <see cref="IJournalListener"/> objects.
    /// </summary>
    [ExcludeFromCodeCoverage]
    public class JournalListenerCollection : IEnumerable<IJournalListener>
    {
        /// <summary>
        /// The collection of listeners.
        /// </summary>
        private readonly HashSet<IJournalListener> _listeners = new HashSet<IJournalListener>();

        /// <summary>
        /// The <see cref="MultiAccessLock"/> used to synchronize access to the object across
        /// multiple threads.
        /// </summary>
        private readonly MultiAccessLock _syncLock = new MultiAccessLock();

        /// <summary>
        /// A hash set of listener types that tracks whether or not a listener, or one of its 
        /// derived types, is present in the collection.
        /// </summary>
        private readonly HashSet<Type> _typeSupportMap = new HashSet<Type>();

        /// <summary>
        /// Initializes a new instance of the <see cref="JournalListenerCollection"/> class. 
        /// </summary>
        internal JournalListenerCollection() { }
 
        /// <summary>
        /// Adds a listener to the collection.
        /// </summary>
        /// <param name="listener">The listener object to add.</param>
        public void Add(IJournalListener listener)
        {
            ArgumentValidator.ValidateNotNull(() => listener);
            using (_syncLock.GetWriterAccess())
            {
                _listeners.Add(listener);
                _typeSupportMap.Add(listener.GetType());
            }
        }

        /// <summary>
        /// Removes all of the listeners from the collection.
        /// </summary>
        public void Clear()
        {
            using (_syncLock.GetWriterAccess())
            {
                _listeners.Clear();
                _typeSupportMap.Clear();
            }
        }

        /// <summary>
        /// Returns an enumerator that iterates through the collection.
        /// </summary>
        /// <returns>A <see cref="IEnumerator{T}"/> that can be used to iterate through the collection.</returns>
        public IEnumerator<IJournalListener> GetEnumerator()
        {
            using (_syncLock.GetReaderAccess())
            {
                var collection = new IJournalListener[_listeners.Count];
                _listeners.CopyTo(collection);
                return collection.OfType<IJournalListener>().GetEnumerator();
            }
        }

        /// <summary>
        /// Returns an enumerator that iterates through a collection.
        /// </summary>
        /// <returns>An <see cref="IEnumerator"/> object that can be used to iterate through the collection.</returns>
        IEnumerator IEnumerable.GetEnumerator() { return this.GetEnumerator(); }

        /// <summary>
        /// Determines whether or not a listener of a given type is present in the collection.
        /// </summary>
        /// <typeparam name="TListener">The type of the listener.</typeparam>
        /// <returns>True if a listener exists; otherwise, false.</returns>
        public bool HasListenerOfType<TListener>() => this.HasListenerOfType(typeof(TListener));

        /// <summary>
        /// Determines whether or not a listener of a given type is present in the collection.
        /// </summary>
        /// <param name="listenerType">The type of the listener.</param>
        /// <returns>True if a listener exists; otherwise, false.</returns>
        public virtual bool HasListenerOfType(Type listenerType)
        {
            using (_syncLock.GetReaderAccess())
            {
                return _typeSupportMap.Where(ti => listenerType.IsAssignableFrom(ti)).Any();
            }
        }
    }
}
