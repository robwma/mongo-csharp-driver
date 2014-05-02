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

using MongoDB.Bson;
using MongoDB.Bson.Serialization;
using MongoDB.Bson.Serialization.Serializers;

namespace MongoDB.Driver.GeoJsonObjectModel.Serializers
{
    /// <summary>
    /// Represents a serializer for a GeoJson3DGeographicCoordinates value.
    /// </summary>
    public class GeoJson3DGeographicCoordinatesSerializer : ClassSerializerBase<GeoJson3DGeographicCoordinates>
    {
        // private static fields
        private static readonly IBsonSerializer<double> __doubleSerializer = new DoubleSerializer();

        // public methods
        /// <summary>
        /// Deserializes a value.
        /// </summary>
        /// <param name="context">The deserialization context.</param>
        /// <returns>The value.</returns>
        public override GeoJson3DGeographicCoordinates Deserialize(BsonDeserializationContext context)
        {
            var bsonReader = context.Reader;

            if (bsonReader.GetCurrentBsonType() == BsonType.Null)
            {
                bsonReader.ReadNull();
                return null;
            }
            else
            {
                bsonReader.ReadStartArray();
                var longitude = context.DeserializeWithChildContext(__doubleSerializer);
                var latitude = context.DeserializeWithChildContext(__doubleSerializer);
                var altitude = context.DeserializeWithChildContext(__doubleSerializer);
                bsonReader.ReadEndArray();

                return new GeoJson3DGeographicCoordinates(longitude, latitude, altitude);
            }
        }

        /// <summary>
        /// Serializes a value.
        /// </summary>
        /// <param name="context">The serialization context.</param>
        /// <param name="value">The value.</param>
        protected override void SerializeValue(BsonSerializationContext context, GeoJson3DGeographicCoordinates value)
        {
            var bsonWriter = context.Writer;

            bsonWriter.WriteStartArray();
            bsonWriter.WriteDouble(value.Longitude);
            bsonWriter.WriteDouble(value.Latitude);
            bsonWriter.WriteDouble(value.Altitude);
            bsonWriter.WriteEndArray();
        }
    }
}
