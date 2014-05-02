﻿/* Copyright 2010-2014 MongoDB Inc.
*
* Licensed under the Apache License, Version 2.0 (the "License");
* you may not use this file except in compliance with the License.
* You may obtain a copy of the License at
*
* http://www.apache.org/licenses/LICENSE-2.0
*
* Unless required by applicable law or agreed to in writing, software
* distributed under the License is distributed on an "AS IS" BASIS,
* WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
* See the License for the specific language governing permissions and
* limitations under the License.
*/

using System;
using MongoDB.Bson.IO;
using MongoDB.Bson.Serialization.Conventions;

namespace MongoDB.Bson.Serialization.Serializers
{
    /// <summary>
    /// Represents a serializer that serializes values as a discriminator/value pair.
    /// </summary>
    /// <typeparam name="TValue">The type of the value.</typeparam>
    public class DiscriminatedWrapperSerializer<TValue> : SerializerBase<TValue>
    {
        // private fields
        private readonly IDiscriminatorConvention _discriminatorConvention;
        private readonly IBsonSerializer<TValue> _wrappedSerializer;

        // constructors
        /// <summary>
        /// Initializes a new instance of the <see cref="DiscriminatedWrapperSerializer{TValue}" /> class.
        /// </summary>
        /// <param name="discriminatorConvention">The discriminator convention.</param>
        /// <param name="wrappedSerializer">The wrapped serializer.</param>
        public DiscriminatedWrapperSerializer(IDiscriminatorConvention discriminatorConvention, IBsonSerializer<TValue> wrappedSerializer)
        {
            _discriminatorConvention = discriminatorConvention;
            _wrappedSerializer = wrappedSerializer;
        }

        // public methods
        /// <summary>
        /// Deserializes a value.
        /// </summary>
        /// <param name="context">The deserialization context.</param>
        /// <returns>An object.</returns>
        public override TValue Deserialize(BsonDeserializationContext context)
        {
            var bsonReader = context.Reader;
            var nominalType = context.NominalType;
            var actualType = _discriminatorConvention.GetActualType(bsonReader, nominalType);

            bsonReader.ReadStartDocument();

            var firstElementName = bsonReader.ReadName();
            if (firstElementName != _discriminatorConvention.ElementName)
            {
                var message = string.Format("Expected the first field of a discriminated wrapper to be '{0}', not: '{1}'.", _discriminatorConvention.ElementName, firstElementName);
                throw new FormatException(message);
            }
            bsonReader.SkipValue();

            var secondElementName = bsonReader.ReadName();
            if (secondElementName != "_v")
            {
                var message = string.Format("Expected the second field of a discriminated wrapper to be '_v', not: '{0}'.", firstElementName);
                throw new FormatException(message);
            }

            var serializer = BsonSerializer.LookupSerializer(actualType);
            var value = serializer.Deserialize(context.CreateChild(actualType));

            if (bsonReader.ReadBsonType() != BsonType.EndOfDocument)
            {
                var message = string.Format("Expected a discriminated wrapper to be a document with exactly two fields, '{0}' and '_v'.", _discriminatorConvention.ElementName);
                throw new FormatException(message);
            }

            bsonReader.ReadEndDocument();

            return (TValue)value;
        }

        /// <summary>
        /// Determines whether the reader is positioned at a discriminated wrapper.
        /// </summary>
        /// <param name="context">The context.</param>
        /// <returns>True if the reader is positioned at a discriminated wrapper.</returns>
        public bool IsPositionedAtDiscriminatedWrapper(BsonDeserializationContext context)
        {
            var bsonReader = context.Reader;
            var bookmark = bsonReader.GetBookmark();

            try
            {
                if (bsonReader.GetCurrentBsonType() != BsonType.Document) { return false; }
                bsonReader.ReadStartDocument();
                if (bsonReader.ReadBsonType() == BsonType.EndOfDocument) { return false; }
                if (bsonReader.ReadName() != _discriminatorConvention.ElementName) { return false; }
                bsonReader.SkipValue();
                if (bsonReader.ReadBsonType() == BsonType.EndOfDocument) { return false; }
                if (bsonReader.ReadName() != "_v") { return false; }
                bsonReader.SkipValue();
                if (bsonReader.ReadBsonType() != BsonType.EndOfDocument) { return false; }
                return true;
            }
            finally
            {
                bsonReader.ReturnToBookmark(bookmark);
            }
        }

        /// <summary>
        /// Serializes a value.
        /// </summary>
        /// <param name="context">The serialization context.</param>
        /// <param name="value">The value.</param>
        public override void Serialize(BsonSerializationContext context, TValue value)
        {
            var bsonWriter = context.Writer;
            var nominalType = context.NominalType;
            var actualType = value.GetType();
            var discriminator = _discriminatorConvention.GetDiscriminator(nominalType, actualType);

            bsonWriter.WriteStartDocument();
            bsonWriter.WriteName(_discriminatorConvention.ElementName);
            context.SerializeWithChildContext(BsonValueSerializer.Instance, discriminator);
            bsonWriter.WriteName("_v");
            _wrappedSerializer.Serialize(context.CreateChild(actualType), value);
            bsonWriter.WriteEndDocument();
        }
    }
}
