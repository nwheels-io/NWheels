using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace NWheels.Processing.Messages
{
    public interface IMessageObject
    {
        void AddHeader(IMessageHeader header);
        Guid MessageId { get; }
        DateTime CreatedAtUtc { get; }
        IReadOnlyCollection<IMessageHeader> Headers { get; }
        object Body { get; }
        Type BodyType { get; }
    }

    //---------------------------------------------------------------------------------------------------------------------------------------------------------

    public static class MessageObjectExtensions
    {
        public static T TryGetHeader<T>(this IMessageObject message) 
            where T : class, IMessageHeader
        {
            if (message.Headers != null)
            {
                return message.Headers.OfType<T>().FirstOrDefault();
            }

            return null;
        }
    }
}
