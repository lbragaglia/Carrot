using System;
using System.Collections.Generic;
using Carrot.Configuration;
using Carrot.Fallback;
using Carrot.Messages;
using Moq;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using RabbitMQ.Client.Framing;
using Xunit;

namespace Carrot.Tests
{
    public class ApplyingFallbackStrategy
    {
        private readonly ConsumingConfiguration _configuration;

        public ApplyingFallbackStrategy()
        {
            _configuration = new ConsumingConfiguration(new Mock<IChannel>().Object, default(Queue));
        }

        [Fact]
        public void OnSuccess()
        {
            var model = new Mock<IModel>();
            var strategy = new Mock<IFallbackStrategy>();
            var args = FakeBasicDeliverEventArgs();
            var message = new FakeConsumedMessage(new Object(), args);
            _configuration.FallbackBy((c, q) => strategy.Object);
            var result = message.ConsumeAsync(_configuration).Result;
            Assert.IsType<Success>(result);
            result.Reply(model.Object);
            strategy.Verify(_ => _.Apply(model.Object, message), Times.Never);
        }

        [Fact]
        public void OnError()
        {
            var model = new Mock<IModel>();
            var strategy = new Mock<IFallbackStrategy>();
            var args = FakeBasicDeliverEventArgs();
            var message = new FakeConsumedMessage(new Object(), args);
            _configuration.FallbackBy((c, q) => strategy.Object);
            _configuration.Consumes(new FakeConsumer(consumedMessage => { throw new Exception(); }));
            var result = message.ConsumeAsync(_configuration).Result;
            Assert.IsType<ConsumingFailure>(result);
            result.Reply(model.Object);
            strategy.Verify(_ => _.Apply(model.Object, message), Times.Never);
        }

        [Fact]
        public void OnReiteratedError()
        {
            var model = new Mock<IModel>();
            var strategy = new Mock<IFallbackStrategy>();
            var args = FakeBasicDeliverEventArgs();
            args.Redelivered = true;
            var message = new FakeConsumedMessage(new Object(), args);
            _configuration.FallbackBy((c, q) => strategy.Object);
            _configuration.Consumes(new FakeConsumer(consumedMessage => { throw new Exception(); }));
            var result = message.ConsumeAsync(_configuration).Result;
            Assert.IsType<ReiteratedConsumingFailure>(result);
            result.Reply(model.Object);
            strategy.Verify(_ => _.Apply(model.Object, message), Times.Once);
        }

        [Fact]
        public void DeadLetterExchangeStrategy()
        {
            var args = FakeBasicDeliverEventArgs();
            var message = new FakeConsumedMessage(null, args);
            var channel = new Mock<IChannel>();
            var queue = new Queue("queue_name");
            Func<String, String> f = _ => String.Format("{0}-DeadLetter", _);
            var dleName = f(queue.Name);
            channel.Setup(_ => _.DeclareDurableDirectExchange(dleName)).Returns(new Exchange(dleName, "direct"));
            var strategy = DeadLetterStrategy.New(channel.Object,
                                                  queue,
                                                  _ => String.Format("{0}-DeadLetter", _));
            var model = new Mock<IModel>();
            strategy.Apply(model.Object, message);
            model.Verify(_ => _.BasicPublish(dleName,
                                             String.Empty,
                                             It.Is<IBasicProperties>(properties => properties.Persistent == true),
                                             args.Body),
                         Times.Once);
        }

        private static BasicDeliverEventArgs FakeBasicDeliverEventArgs()
        {
            return new BasicDeliverEventArgs
                       {
                           BasicProperties = new BasicProperties
                                                 {
                                                     Headers = new Dictionary<String, Object>()
                                                 }
                       };
        }
    }
}