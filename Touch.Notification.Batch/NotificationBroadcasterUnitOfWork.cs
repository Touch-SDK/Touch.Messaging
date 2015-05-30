using System;
using System.Collections.Generic;
using System.ServiceModel;
using Touch.ServiceModel;

namespace Touch.Notification
{
    public sealed class NotificationBroadcasterUnitOfWork<T> : IUnitOfWork
        where T : class, new()
    {
        public NotificationBroadcasterUnitOfWork(Func<INotificationBroadcaster<T>> factory)
        {
            _factory = factory;
        }

        private readonly Func<INotificationBroadcaster<T>> _factory;

        public void Commit()
        {
            try
            {
                if (Notifications.Count > 0)
                {
                    var broadcaster = _factory();

                    if (broadcaster == null)
                        throw new OperationCanceledException("Missing broadcaster for notifications of type " + typeof (T).FullName);

                    foreach (var notification in Notifications)
                        broadcaster.Broadcast(notification);
                }
            }
            finally
            {
                ClearNotifications();
            }
        }

        public void Dispose()
        {
            ClearNotifications();
        }

        private static IList<T> Notifications
        {
            get
            {
                object value;
                return OperationContext.Current != null &&
                       OperationContext.Current.IncomingMessageProperties.TryGetValue(BatchNotificationBroadcaster<T>.DataKey, out value)
                    ? (IList<T>)value
                    : new T[0];
            }
        }

        private static void ClearNotifications()
        {
            if (OperationContext.Current != null)
                OperationContext.Current.IncomingMessageProperties.Remove(BatchNotificationBroadcaster<T>.DataKey);
        }
    }
}
