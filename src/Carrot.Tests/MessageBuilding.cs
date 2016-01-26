using System;
using System.Collections.Generic;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using RabbitMQ.Client.Framing;
using Xunit;

namespace Carrot.Tests
{
    public class MessageBuilding
    {
        [Fact]
        public void ProperContentType()
        {
            var content = new Foo();
            var message = new FakeConsumedMessage(content, FakeBasicDeliverEventArgs());
            var actual = message.As<Foo>();
            Assert.Equal(content, actual.Content);
        }

        [Fact]
        public void WrongContentType()
        {
            var content = new Foo();
            var message = new FakeConsumedMessage(content, FakeBasicDeliverEventArgs());
            Assert.Throws<InvalidCastException>(() => message.As<Bar>());
        }

        [Fact]
        public void HeaderMapping()
        {
            var content = new Foo();
            const String messageId = "one-id";
            const Int64 timestamp = 123456789L;
            const String replyTo = "reply-queue-name";
            String correlationId = Guid.NewGuid().ToString();
            var args = new BasicDeliverEventArgs
            {
                BasicProperties = new BasicProperties
                {
                    MessageId = messageId,
                    Timestamp = new AmqpTimestamp(timestamp),
                    CorrelationId = correlationId,
                    ReplyTo = replyTo,
                }
            };
            var message = new FakeConsumedMessage(content, args);
            var actual = message.As<Foo>();
            Assert.Equal(messageId, actual.Headers.MessageId);
            Assert.Equal(timestamp, actual.Headers.Timestamp);
            Assert.Equal(correlationId, actual.Headers.CorrelationId);
            Assert.Equal(replyTo, actual.Headers.ReplyTo);
        }

        [Fact]
        public void ConsumerTag()
        {
            var content = new Foo();
            const String consumerTag = "one-tag";
            var args = new BasicDeliverEventArgs
                           {
                               ConsumerTag = consumerTag,
                               BasicProperties = new BasicProperties()
                           };
            var message = new FakeConsumedMessage(content, args);
            var actual = message.As<Foo>();
            Assert.Equal(consumerTag, actual.ConsumerTag);
        }

        [Fact]
        public void CustomHeader()
        {
            var content = new Foo();
            var args = new BasicDeliverEventArgs
                           {
                               BasicProperties = new BasicProperties
                                                     {
                                                         Headers = new Dictionary<String, Object>
                                                                       {
                                                                           { "a", "b" }
                                                                       }
                                                     }
                           };
            var message = new FakeConsumedMessage(content, args);
            var actual = message.As<Foo>();
            Assert.Equal("b", actual.Headers["a"]);
        }

        private static BasicDeliverEventArgs FakeBasicDeliverEventArgs()
        {
            return new BasicDeliverEventArgs
            {
                BasicProperties = new BasicProperties()
            };
        }
    }
}