using System;
using CPP.Framework.DependencyInjection;
using CPP.Framework.Threading;

namespace CPP.Framework
{
    /// <summary>
    /// Manages random values and contexts for the application.
    /// </summary>
    public class Randomizer : SingletonServiceBase
    {
        /// <summary>
        /// The current instance of the service for the application.
        /// </summary>
        private static readonly ServiceInstance<Randomizer> _ServiceInstance = new ServiceInstance<Randomizer>();

        /// <summary>
        /// The <see cref="MultiAccessLock"/> used to synchronize access to the object across
        /// multiple threads.
        /// </summary>
        private readonly object _syncLock = new object();

        /// <summary>
        /// The random number generator for the object.
        /// </summary>
        private Random _random;

        /// <summary>
        /// Gets the current instance of the service for the application.
        /// </summary>
        public static Randomizer Current => _ServiceInstance.GetInstance();

        /// <summary>
        /// Generates a non-negative random value.
        /// </summary>
        /// <returns>A random <see cref="Int32"/> value that is greater than or equal to zero, and less than or equal to <see cref="int.MaxValue"/>.</returns>
        public int Next() => this.Next(0, int.MaxValue);

        /// <summary>
        /// Generates a non-negative random value that is less than the specified maximum.
        /// </summary>
        /// <param name="max">The highest (maximum) value to return.</param>
        /// <returns>A random <see cref="Int32"/> value that is greater than or equal to zero, and less than or equal to <paramref name="max"/>.</returns>
        /// <exception cref="ArgumentOutOfRangeException"><paramref name="max"/> is less than zero.</exception>
        public int Next(int max)
        {
            if (max < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(max));
            }
            return this.Next(0, max);
        }

        /// <summary>
        /// Generates a random value within a range.
        /// </summary>
        /// <param name="min">The lowest (minimum) value to return.</param>
        /// <param name="max">The highest (maximum) value to return.</param>
        /// <returns>A random <see cref="Int32"/> value that is greater than or equal to <paramref name="min"/>, and less than or equal to <paramref name="max"/>.</returns>
        /// <exception cref="ArgumentOutOfRangeException"><paramref name="max"/> is less than <paramref name="min"/>.</exception>
        public virtual int Next(int min, int max)
        {
            lock (_syncLock)
            {
                return _random.Next(min, max);
            }
        }

        /// <summary>
        /// Called by the base class to perform any initialization tasks when the instance is being
        /// created.
        /// </summary>
        protected internal override void StartupInstance()
        {
            lock (_syncLock)
            {
                if (!ServiceLocator.TryGetInstance<Random>(out _random))
                {
                    _random = new Random();
                }
            }
            base.StartupInstance();
        }
    }
}
