using System;
using System.Collections.Generic;
using Carrot.Configuration;
using Carrot.Fallback;
using Carrot.Messages;
using Carrot.Serialization;
using RabbitMQ.Client;

namespace Carrot
{
    public class MessageQueue : IEquatable<MessageQueue>
    {
        private readonly String _name;
        private readonly IModel _model;
        private readonly IMessageTypeResolver _resolver;
        private readonly ISerializerFactory _serializerFactory;

        // TODO: restore private
        internal MessageQueue(String name,
                              IModel model,
                              IMessageTypeResolver resolver,
                              ISerializerFactory serializerFactory)
        {
            _name = name;
            _model = model;
            _resolver = resolver;
            _serializerFactory = serializerFactory;
        }

        internal String Name
        {
            get { return _name; }
        }

        public static Boolean operator ==(MessageQueue left, MessageQueue right)
        {
            return Equals(left, right);
        }

        public static Boolean operator !=(MessageQueue left, MessageQueue right)
        {
            return !Equals(left, right);
        }

        public Boolean Equals(MessageQueue other)
        {
            if (ReferenceEquals(null, other))
                return false;

            if (ReferenceEquals(this, other))
                return true;

            return String.Equals(_name, other._name);
        }

        public override Boolean Equals(Object obj)
        {
            if (ReferenceEquals(null, obj))
                return false;

            if (ReferenceEquals(this, obj))
                return true;

            var other = obj as MessageQueue;
            return other != null && Equals(other);
        }

        public override Int32 GetHashCode()
        {
            return _name.GetHashCode();
        }

        public void SubscribeByAtMostOnce(Action<SubscriptionConfiguration> configure)
        {
            SubscribeByAtMostOnce(configure, NoFallbackStrategy.Instance);
        }

        public void SubscribeByAtMostOnce(Action<SubscriptionConfiguration> configure,
                                          IFallbackStrategy fallbackStrategy)
        {
            Subscribe(configure,
                      (b, c) => new AtMostOnceConsumer(_model, b, c),
                      fallbackStrategy,
                      _serializerFactory);
        }

        public void SubscribeByAtLeastOnce(Action<SubscriptionConfiguration> configure)
        {
            SubscribeByAtLeastOnce(configure, NoFallbackStrategy.Instance);
        }

        public void SubscribeByAtLeastOnce(Action<SubscriptionConfiguration> configure,
                                           IFallbackStrategy fallbackStrategy)
        {
            Subscribe(configure,
                      (b, c) => new AtLeastOnceConsumer(_model, b, c),
                      fallbackStrategy,
                      _serializerFactory);
        }

        internal static MessageQueue New(IModel model,
                                         IMessageTypeResolver resolver,
                                         ISerializerFactory serializerFactory,
                                         String name,
                                         Exchange exchange,
                                         String routingKey = "")
        {
            var queue = new MessageQueue(name, model, resolver, serializerFactory);

            exchange.Declare(model);
            model.QueueDeclare(name, true, false, false, new Dictionary<String, Object>());
            exchange.Bind(queue, model, routingKey);

            return queue;
        }

        private void Subscribe(Action<SubscriptionConfiguration> configure,
                               Func<IConsumedMessageBuilder, SubscriptionConfiguration, ConsumerBase> func,
                               IFallbackStrategy fallbackStrategy,
                               ISerializerFactory serializerFactory)
        {
            var configuration = new SubscriptionConfiguration(fallbackStrategy);
            configure(configuration);
            var builder = new ConsumedMessageBuilder(serializerFactory, _resolver);

            _model.BasicConsume(_name, false, func(builder, configuration));
        }
    }
}