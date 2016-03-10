using NWheels.Processing.Messages;

namespace NWheels.Serialization
{
    public interface IObjectSerializer
    {
        object Deserialize(IMessageObject message);
    }
}
