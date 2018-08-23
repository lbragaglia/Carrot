using System;
using System.Collections.Generic;
using Carrot.Serialization;

namespace Carrot.Configuration
{
    public class SerializationConfiguration
    {
        internal const String DefaultContentType = "application/json";
        internal const String DefaultContentEncoding = "UTF-8";

        private readonly IDictionary<ContentNegotiator.MediaType, ISerializer> _mediaTypeSerializers =
            new Dictionary<ContentNegotiator.MediaType, ISerializer>();

        private readonly IDictionary<Predicate<ContentNegotiator.MediaTypeHeader>, ISerializer> _serializers =
            new Dictionary<Predicate<ContentNegotiator.MediaTypeHeader>, ISerializer>();

        private IContentNegotiator _negotiator = new ContentNegotiator();

        internal static SerializationConfiguration Default()
        {
            var defaultConfig = new SerializationConfiguration();
            defaultConfig._serializers[_ => _.MediaType == DefaultContentType] = new JsonSerializer();
            return defaultConfig;
        }

        internal SerializationConfiguration() { }

        public void Map(Predicate<ContentNegotiator.MediaTypeHeader> predicate, ISerializer serializer)
        {
            if (predicate == null)
                throw new ArgumentNullException(nameof(predicate));

            if (serializer == null)
                throw new ArgumentNullException(nameof(serializer));

            _serializers.Add(predicate, serializer);
        }

        public void Map(ContentNegotiator.MediaType mediaType, ISerializer serializer)
        {
            if (mediaType == null)
                throw new ArgumentNullException(nameof(mediaType));

            if (serializer == null)
                throw new ArgumentNullException(nameof(serializer));

            _mediaTypeSerializers[mediaType] = serializer;
        }

        public void NegotiateBy(IContentNegotiator negotiator)
        {
            _negotiator = negotiator ?? throw new ArgumentNullException(nameof(negotiator));
        }

        internal virtual ISerializer Create(String contentType)
        {
            if (contentType == null)
                throw new ArgumentNullException(nameof(contentType));

            var sortedMediaTypes = _negotiator.Negotiate(contentType);

            foreach (var header in sortedMediaTypes)
                if (_mediaTypeSerializers.ContainsKey(header.MediaType))
                    return _mediaTypeSerializers[header.MediaType];
                else
                    foreach (var serializer in _serializers)
                        if (serializer.Key(header))
                            return serializer.Value;

            return NullSerializer.Instance;
        }
    }
}