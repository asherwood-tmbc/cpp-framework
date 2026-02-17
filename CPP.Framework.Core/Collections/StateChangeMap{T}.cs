using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;

namespace CPP.Framework.Collections
{
    /// <summary>
    /// Stores and validates the possible transition states from one status code to another.
    /// </summary>
    /// <typeparam name="T">The type of the transition state.</typeparam>
    [ExcludeFromCodeCoverage]
    public sealed class StateChangeMap<T>
    {
        /// <summary>
        /// The map of state values to their allowed transition sets.
        /// </summary>
        private readonly Dictionary<T, HashSet<T>> _transitionStateMap;

        /// <summary>
        /// Initializes a new instance of the <see cref="StateChangeMap{T}"/> class using the 
        /// default equality comparer for the type.
        /// </summary>
        public StateChangeMap() : this(null, null) { }

        /// <summary>
        /// Initializes a new instance of the <see cref="StateChangeMap{T}"/> class
        /// </summary>
        /// <param name="comparer">The <see cref="IEqualityComparer{T}"/> to use when comparing values.</param>
        public StateChangeMap(IEqualityComparer<T> comparer) : this(null, comparer) { }

        /// <summary>
        /// Initializes a new instance of the <see cref="StateChangeMap{T}"/> class using the 
        /// default equality comparer for the type.
        /// </summary>
        /// <param name="source">A source dictionary that contains the initial transition state values.</param>
        public StateChangeMap(IEnumerable<KeyValuePair<T, IEnumerable<T>>> source) : this(source, null) { }

        /// <summary>
        /// Initializes a new instance of the <see cref="StateChangeMap{T}"/> class.
        /// </summary>
        /// <param name="source">A source dictionary that contains the initial transition state values.</param>
        /// <param name="comparer">The <see cref="IEqualityComparer{T}"/> to use when comparing values.</param>
        public StateChangeMap(IEnumerable<KeyValuePair<T, IEnumerable<T>>> source, IEqualityComparer<T> comparer)
        {
            var map = new Dictionary<T, HashSet<T>>(comparer ?? EqualityComparer<T>.Default);
            if (source != null)
            {
                foreach (var entry in source)
                {
                    map[entry.Key] = new HashSet<T>(entry.Value, comparer);
                }
            }
            _transitionStateMap = map;
        }

        /// <summary>
        /// Defines the possible values available when transitioning from one code to another.
        /// </summary>
        /// <param name="state">The status code to map.</param>
        /// <param name="allowed">A list of one or more codes that are valid transition values for <paramref name="state"/>.</param>
        public void DefineStateChange(T state, params T[] allowed)
        {
            _transitionStateMap[state] = new HashSet<T>(allowed, _transitionStateMap.Comparer);
        }

        /// <summary>
        /// Verifies whether or not a state change is valid.
        /// </summary>
        /// <param name="current">The current state value.</param>
        /// <param name="proposed">The new proposed state value.</param>
        /// <returns>True if the state change is valid; otherwise, false.</returns>
        public bool IsValid(T current, T proposed)
        {
            if (_transitionStateMap.Comparer.Equals(current, proposed))
            {
                return true;
            }
            if (_transitionStateMap.TryGetValue(current, out var transitions))
            {
                return transitions.Contains(proposed);
            }
            return false;
        }

        /// <summary>
        /// Validates the a state transition is valid.
        /// </summary>
        /// <param name="current">The current state value.</param>
        /// <param name="proposed">The new proposed state value.</param>
        /// <returns>The value of the <paramref name="proposed"/> if the state change is valid.</returns>
        /// <exception cref="InvalidStateChangeException{T}"><paramref name="proposed"/> is not a valid state transition for <paramref name="current"/>.</exception>
        public T ValidateTransition(T current, T proposed)
        {
            if (!this.IsValid(current, proposed))
            {
                throw new InvalidStateChangeException<T>(current, proposed);
            }
            return proposed;
        }
    }
}
