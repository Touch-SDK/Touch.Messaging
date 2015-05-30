using System;
using System.Collections.Generic;
using System.ServiceModel;

namespace Touch.Notification
{
    public sealed class BatchNotificationBroadcaster<T> : INotificationBroadcaster<T> 
        where T : class, new()
    {
        public static string DataKey = "Broadcaster: " + typeof(T).FullName;

        public void Broadcast(T notification)
        {
            if (OperationContext.Current == null)
                throw new InvalidOperationException("Missing WCF operation context.");

            object values;
            var container = new List<T>();

            if (!OperationContext.Current.IncomingMessageProperties.TryGetValue(DataKey, out values))
                OperationContext.Current.IncomingMessageProperties.Add(DataKey, container);
            else
                container = (List<T>) values;

            container.Add(notification);
        }

        public int Count
        {
            get
            {
                object values;
                if (OperationContext.Current == null || !OperationContext.Current.IncomingMessageProperties.TryGetValue(DataKey, out values))
                    return 0;
                
                return ((List<T>)values).Count;
            }
        }
    }
}
