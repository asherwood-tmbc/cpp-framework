using System;
using System.Threading;

namespace CPP.Framework.Threading
{
    /// <summary>
    /// Single-Use class that is used to limit the amount of I/O and CPU resources that are 
    /// consumed by each iteration of a processing loop for a single thread. Please note that
    /// instances of this class are intended to be used once by a single thread within a single 
    /// processing loop, and therefore are not thread safe.
    /// </summary>
    public class ThreadLimiter
    {
        /// <summary>
        /// The default maximum amount of time to sleep between iterations.
        /// </summary>
        private const int DefaultMaximumSleepPeriod = 300;

        /// <summary>
        /// The default minimum amount of time to sleep between iterations.
        /// </summary>
        private const int DefaultMinimumSleepPeriod = 100;

        /// <summary>
        /// The absolute interval to use for each sleep period, in milliseconds.
        /// </summary>
        private readonly bool _hasMatchingPeriods;

        /// <summary>
        /// The number of iterations that must be complete on the thread between each sleep period.
        /// </summary>
        private readonly ulong _itemSleepThreshold;

        /// <summary>
        /// The maximum amount of time to sleep between iterations.
        /// </summary>
        private readonly int _maximumSleepPeriod;

        /// <summary>
        /// The default minimum amount of time to sleep between iterations.
        /// </summary>
        private readonly int _minimumSleepPeriod;

        /// <summary>
        /// The counter value for the number of loop iterations.
        /// </summary>
        private ulong _itemCounter;

        /// <summary>
        /// Initializes a new instance of the <see cref="ThreadLimiter"/> class. 
        /// </summary>
        /// <param name="threshold">
        /// The number of iterations that must be complete on the thread between each sleep period.
        /// </param>
        public ThreadLimiter(ulong threshold) : this(threshold, DefaultMinimumSleepPeriod, DefaultMaximumSleepPeriod) { }

        /// <summary>
        /// Initializes a new instance of the <see cref="ThreadLimiter"/> class. 
        /// </summary>
        /// <param name="threshold">
        /// The number of iterations that must be complete on the thread between each sleep period.
        /// </param>
        /// <param name="period">
        /// The absolute interval to use for each sleep period, in milliseconds.
        /// </param>
        public ThreadLimiter(ulong threshold, int period) : this(threshold, period, period) { }

        /// <summary>
        /// Initializes a new instance of the <see cref="ThreadLimiter"/> class. 
        /// Initializes an instance of the class.
        /// </summary>
        /// <param name="threshold">
        /// The number of iterations that must be complete on the thread between each sleep period.
        /// </param>
        /// <param name="minPeriod">
        /// The minimum amount of time for each sleep period, in milliseconds.
        /// </param>
        /// <param name="maxPeriod">
        /// The maximum amount of time for each sleep period, in milliseconds.
        /// </param>
        public ThreadLimiter(ulong threshold, int minPeriod, int maxPeriod)
        {
            if (minPeriod < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(minPeriod));
            }
            if (maxPeriod < 0 || maxPeriod < minPeriod)
            {
                throw new ArgumentOutOfRangeException(nameof(maxPeriod));
            }
            threshold = ((threshold <= 0) ? 0 : threshold);

            _hasMatchingPeriods = (minPeriod == maxPeriod);
            _itemSleepThreshold = threshold;
            _maximumSleepPeriod = maxPeriod;
            _minimumSleepPeriod = minPeriod;
        }

        /// <summary>
        /// Increments the counter for the current processing iteration, and sleeps for a random
        /// period within the specified range, if necessary.
        /// </summary>
        public virtual void CompleteIteration() { this.CompleteIteration(false); }

        /// <summary>
        /// Increments the counter for the current processing iteration, and sleeps for a random
        /// period within the specified range, if necessary.
        /// </summary>
        /// <param name="force">True to force the thread to sleep, regardless of the iteration counter; otherwise, false.</param>
        public virtual void CompleteIteration(bool force)
        {
            if (!force && (_itemSleepThreshold >= 2))
            {
                // only sleep if the iteration calls for it.
                if ((++_itemCounter % _itemSleepThreshold) != 0) return;
            }
            var interval = _minimumSleepPeriod;

            if (!_hasMatchingPeriods)
            {
                // if the min an max periods don't match, then we need a random value between them.
                interval = Randomizer.Current.Next(_minimumSleepPeriod, _maximumSleepPeriod);
            }
            Thread.Sleep(interval);
        }
    }
}
