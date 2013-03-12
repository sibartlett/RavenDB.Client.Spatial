using System;
using Raven.Imports.Newtonsoft.Json;

namespace Raven.Client.Spatial
{
	internal class GeoJsonWriter
	{
		private readonly IShapeConverter _shapeConverter;

		public GeoJsonWriter(IShapeConverter shapeConverter)
		{
			_shapeConverter = shapeConverter;
		}

		public void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
		{
			if (value == null)
			{
				writer.WriteValue((object)null);
				return;
			}

			if (TryWriteGeometry(writer, value, serializer))
				return;

			if (TryWriteGeometryCollection(writer, value, serializer))
				return;

			throw new Exception("Error!");
		}

		public bool TryWriteGeometryCollection(JsonWriter writer, object value, JsonSerializer serializer)
		{
			if (!_shapeConverter.IsValid(value))
				return false;

			GeoJsonObjectType geomType = _shapeConverter.GetGeoJsonObjectType(value);
			if (geomType != GeoJsonObjectType.GeometryCollection)
				return false;

			writer.WriteStartObject();
			writer.WritePropertyName("type");
			writer.WriteValue(Enum.GetName(typeof(GeoJsonObjectType), geomType));

			writer.WritePropertyName("geometries");
			writer.WriteStartArray();
			foreach (var geometry in _shapeConverter.FromGeometryCollection(value))
				serializer.Serialize(writer, geometry);
			writer.WriteEndArray();

			writer.WriteEndObject();
			return true;
		}

		public bool TryWriteGeometry(JsonWriter writer, object value, JsonSerializer serializer)
		{
			if (! _shapeConverter.IsValid(value))
				return false;

			GeoJsonObjectType geomType = _shapeConverter.GetGeoJsonObjectType(value);

			switch (geomType)
			{
				case GeoJsonObjectType.Point:
				case GeoJsonObjectType.LineString:
				case GeoJsonObjectType.Polygon:
				case GeoJsonObjectType.MultiPoint:
				case GeoJsonObjectType.MultiLineString:
				case GeoJsonObjectType.MultiPolygon:
					break;
				default:
					return false;
			}

			writer.WriteStartObject();
			writer.WritePropertyName("type");
			writer.WriteValue(Enum.GetName(typeof(GeoJsonObjectType), geomType));

			writer.WritePropertyName("coordinates");

			switch (geomType)
			{
				case GeoJsonObjectType.Point:
					WriteJsonCoordinate(writer, _shapeConverter.FromPoint(value));
					break;
				case GeoJsonObjectType.LineString:
					WriteJsonCoordinates(writer, _shapeConverter.FromLineString(value));
					break;

				case GeoJsonObjectType.MultiPoint:
					WriteJsonCoordinates(writer, _shapeConverter.FromMultiPoint(value));
					break;

				case GeoJsonObjectType.Polygon:
					WriteJsonCoordinatesEnumerable(writer, _shapeConverter.FromPolygon(value));
					break;

				case GeoJsonObjectType.MultiLineString:
					WriteJsonCoordinatesEnumerable(writer, _shapeConverter.FromMultiLineString(value));
					break;

				case GeoJsonObjectType.MultiPolygon:
					WriteJsonCoordinatesEnumerable2(writer, _shapeConverter.FromMultiPolygon(value));
					break;
			}

			writer.WriteEndObject();
			return true;
		}

		private static void WriteJsonCoordinate(JsonWriter writer, CoordinateInfo coordinate)
		{
			writer.WriteStartArray();

			writer.WriteValue(coordinate.X);
			writer.WriteValue(coordinate.Y);

			if (coordinate.Z.HasValue)
				writer.WriteValue(coordinate.Z.Value);

            if (coordinate.Z.HasValue && coordinate.M.HasValue)
				writer.WriteValue(coordinate.M.Value);

			writer.WriteEndArray();
		}

        private static void WriteJsonCoordinates(JsonWriter writer, CoordinateInfo[] coordinates)
		{
			writer.WriteStartArray();
			foreach (var coordinate in coordinates)
				WriteJsonCoordinate(writer, coordinate);
			writer.WriteEndArray();
		}

        private static void WriteJsonCoordinatesEnumerable(JsonWriter writer, CoordinateInfo[][] coordinates)
		{
			writer.WriteStartArray();
			foreach (var coordinate in coordinates)
				WriteJsonCoordinates(writer, coordinate);
			writer.WriteEndArray();
		}

        private static void WriteJsonCoordinatesEnumerable2(JsonWriter writer, CoordinateInfo[][][] coordinates)
		{
			writer.WriteStartArray();
			foreach (var coordinate in coordinates)
				WriteJsonCoordinatesEnumerable(writer, coordinate);
			writer.WriteEndArray();
		}
	}
}
