using System;
using DotSpatial.Topology;
using Raven.Imports.Newtonsoft.Json;

namespace Raven.Client.Spatial.DotSpatial
{
	public class WktConverter : JsonConverter
	{
		private readonly Spatial4nReader _wktReader;
		private readonly Spatial4nWriter _wktWriter;

		public WktConverter() : this(GeometryFactory.Default)
		{
		}

		public WktConverter(IGeometryFactory geometryFactory)
		{
			var maker = new ShapeConverter(geometryFactory);
			_wktReader = new Spatial4nReader(maker);
			_wktWriter = new Spatial4nWriter(maker);
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
			return typeof(IGeometry).IsAssignableFrom(objectType) || typeof(Envelope) == objectType;
		}
	}
}
