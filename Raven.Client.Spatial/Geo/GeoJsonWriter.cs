using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Geo.Gps;
using Raven.Imports.Newtonsoft.Json;

namespace Raven.Client.Spatial.Geo
{
	internal class GeoJsonWriter : Spatial.GeoJsonWriter
	{
		private readonly IShapeConverter _shapeConverter;

		public GeoJsonWriter(IShapeConverter shapeConverter) : base(shapeConverter)
		{
			_shapeConverter = shapeConverter;
		}

		public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
		{
			if (TryWriteGeo(writer, value, serializer))
				return;

			base.WriteJson(writer, value, serializer);
		}

		public bool TryWriteGeo(JsonWriter writer, object value, JsonSerializer serializer)
		{
			var route = value as Route;
			if (route != null)
			{
				writer.WriteStartObject();

				writer.WritePropertyName("__geo");
				writer.WriteStartObject();
				writer.WritePropertyName("type");
				writer.WriteValue("Route");
				if (route.Metadata.Count > 0)
				{
					writer.WritePropertyName("metadata");
					serializer.Serialize(writer, route.Metadata);
				}
				writer.WriteEndObject();

				writer.WritePropertyName("type");
				writer.WriteValue(Enum.GetName(typeof(GeoJsonObjectType), GeoJsonObjectType.LineString));

				writer.WritePropertyName("coordinates");
				WriteJsonCoordinates(writer, _shapeConverter.FromLineString(route.ToLineString()));

				writer.WriteEndObject();
				return true;
			}

			var track = value as Track;
			if (track != null)
			{
				writer.WriteStartObject();

				writer.WritePropertyName("__geo");
				writer.WriteStartObject();
				writer.WritePropertyName("type");
				writer.WriteValue("Track");
				if (track.Metadata.Count > 0)
				{
					writer.WritePropertyName("metadata");
					serializer.Serialize(writer, track.Metadata);
				}
				writer.WritePropertyName("times");
				writer.WriteStartArray();

				foreach (var segment in track.Segments)
				{
					writer.WriteStartArray();
					foreach (var fix in segment.Fixes)
						writer.WriteValue(fix.TimeUtc);
					writer.WriteEndArray();
				}

				writer.WriteEndArray();
				writer.WriteEndObject();

				writer.WritePropertyName("type");
				writer.WriteValue(Enum.GetName(typeof(GeoJsonObjectType), GeoJsonObjectType.LineString));

				writer.WritePropertyName("coordinates");
				WriteJsonCoordinates(writer, _shapeConverter.FromLineString(track.ToLineString()));

				writer.WriteEndObject();
				return true;
			}
			return false;
		}
	}
}
