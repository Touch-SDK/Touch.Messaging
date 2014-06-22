using System;
using System.Timers;

namespace Touch.Queue
{
    /// <summary>
    /// Queue watcher.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class QueueWatcher<T> : IQueueWatcher<T>
        where T : class, IMessage, new()
    {
        #region Data
        /// <summary>
        /// Queue polling interval.
        /// </summary>
        private readonly TimeSpan _pollInterval;

        /// <summary>
        /// New notification handler.
        /// </summary>
        private HandleNewQueueItem<T> _handler;

        /// <summary>
        /// Queue to watch.
        /// </summary>
        private IMessageQueue<T> _queue;

        /// <summary>
        /// Polling timer.
        /// </summary>
        private readonly Timer _timer;
        #endregion;

        #region .ctor
        public QueueWatcher()
            : this(TimeSpan.FromMilliseconds(1000))
        {
        }

        /// <summary>
        /// Creates an instance of the QueueWatcher.
        /// </summary>
        /// <param name="pollInterval">Interval between polling the queue.</param>
        public QueueWatcher(TimeSpan pollInterval)
        {
            if (Math.Abs(pollInterval.TotalMilliseconds - 0) < 1)
                throw new ArgumentException("Invalid interval.", "pollInterval");

            _pollInterval = pollInterval;

            _timer = new Timer(_pollInterval.TotalMilliseconds);
            _timer.Elapsed += OnTimedEvent;
        }
        #endregion;

        #region IQueueWatcher implementation
        public bool IsWatching { get; private set; }

        public void StartWatching(IMessageQueue<T> queue, HandleNewQueueItem<T> handler)
        {
            if (IsWatching)
                throw new InvalidOperationException("Watcher is already active.");

            if (queue == null)
                throw new ArgumentNullException("queue");

            if (handler == null)
                throw new ArgumentNullException("handler");

            IsWatching = true;

            _handler = handler;
            _queue = queue;

            Wait();
        }
        
        public void StopWatching()
        {
            if (!IsWatching)
                throw new InvalidOperationException("Watcher is not active.");

            IsWatching = false;

            _timer.Stop();

            _queue = null;
            _handler = null;
        }

        public void Dispose()
        {
            if (IsWatching)
                StopWatching();
        }
        #endregion;

        #region Private methods
        private void CheckQueue()
        {
            Console.WriteLine("Checking for new messages...");

            var messages = _queue.Dequeue(1, new TimeSpan(0, 0, 0, 15));

            foreach (var notification in messages)
            {
                if (notification != null)
                {
                    _handler(notification.Body);
                }

                break;
            }

            if (IsWatching)
                Wait();
        }

        private void Wait()
        {
            _timer.Start();
        }

        private void OnTimedEvent(object source, ElapsedEventArgs e)
        {
            _timer.Stop();
            CheckQueue();
        }
        #endregion;
    }
}
