using System;
using Geo.Abstractions.Interfaces;
using Raven.Imports.Newtonsoft.Json;

namespace Raven.Client.Spatial.Geo
{
    public class WktConverter : JsonConverter
    {
        private readonly WktReader _wktReader;
        private readonly WktWriter _wktWriter;

		public WktConverter()
		{
			var maker = new ShapeConverter();
			_wktReader = new WktReader(maker);
			_wktWriter = new WktWriter(maker);
		}

        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            if (value == null)
                writer.WriteValue((object)null);
            else
                writer.WriteValue(_wktWriter.Write(value));
        }

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            if (reader.TokenType == JsonToken.Null)
                return null;

            return _wktReader.Read((string)reader.Value);
        }

        public override bool CanConvert(Type objectType)
        {
            return typeof(IGeometry).IsAssignableFrom(objectType);
        }
    }
}
