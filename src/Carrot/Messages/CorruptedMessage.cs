using System;
using System.Threading.Tasks;
using Carrot.Messaging;

namespace Carrot.Messages
{
    public class CorruptedMessage : ConsumedMessageBase
    {
        public CorruptedMessage(String messageId, 
                                UInt64 deliveryTag,
                                Boolean redelivered,
                                Int64 timestamp)
            : base(messageId, deliveryTag, redelivered, timestamp)
        {
        }

        internal override Object Content
        {
            get { return null; }
        }

        internal override Task<IAggregateConsumingResult> ConsumeAsync(SubscriptionConfiguration configuration)
        {
            return Task.FromResult((IAggregateConsumingResult)new CorruptedMessageFailure());
        }

        internal override Boolean Match(Type type)
        {
            return false;
        }
    }

    public class CorruptedMessageFailure : Failure
    {
        internal CorruptedMessageFailure()
        {
        }
    }
}