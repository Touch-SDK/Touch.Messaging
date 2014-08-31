namespace Touch.Queue
{
    internal sealed class QueueItem<T> : IQueueItem<T>
        where T : class, new()
    {
        public string Id { get; set; }

        public string Receipt { get; set; }

        public T Body { get; set; }

        public bool Equals(IQueueItem<T> other) { return other.Id.Equals(Id); }
    }
}
