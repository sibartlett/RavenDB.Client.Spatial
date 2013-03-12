using System.Globalization;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;

namespace Raven.Client.Spatial
{
    internal class WktWriter
    {
		private readonly IShapeConverter _shapeConverter;
        private readonly WktWriterSettings _settings;

		public WktWriter(IShapeConverter shapeConverter)
		{
			_shapeConverter = shapeConverter;
            _settings = new WktWriterSettings();
        }

        public string Write(object geometry)
        {
            var builder = new StringBuilder();
            AppendGeometry(builder, geometry);
            return builder.ToString();
        }

        private void AppendGeometry(StringBuilder builder, object geometry)
        {
	        var type = _shapeConverter.GetWktObjectType(geometry);

			if (type == WktObjectType.Point)
            {
                AppendPoint(builder, geometry);
                return;
            }

			//if (_settings.LinearRing)
			//{
			//	var linearRing = geometry as LinearRing;
			//	if (linearRing != null)
			//	{
			//		AppendLinearRing(builder, linearRing);
			//		return;
			//	}
			//}

            if (type == WktObjectType.LineString)
            {
                AppendLineString(builder, geometry);
                return;
            }

			//if (_settings.Triangle)
			//{
			//	var triangle = geometry as Triangle;
			//	if (triangle != null)
			//	{
			//		AppendTriangle(builder, triangle);
			//		return;
			//	}
			//}

            if (type == WktObjectType.Polygon)
            {
                AppendPolygon(builder, geometry);
                return;
            }

            if (type == WktObjectType.MultiPoint)
            {
                AppendMultiPoint(builder, geometry);
                return;
            }

            if (type == WktObjectType.MultiLineString)
            {
                AppendMultiLineString(builder, geometry);
                return;
            }

            if (type == WktObjectType.MultiPolygon)
            {
                AppendMultiPolygon(builder, geometry);
                return;
            }

            if (type == WktObjectType.GeometryCollection)
            {
                AppendGeometryCollection(builder, geometry);
                return;
            }

            throw new SerializationException("Geometry of type '" + geometry.GetType().Name + "' is not supported");

        }

        private void AppendPoint(StringBuilder builder, object point)
        {
            var p = _shapeConverter.FromPoint(point);
            builder.Append("POINT");
            if (p != null)
                AppendDimensions(builder, p.GetDimensions());
            builder.Append(" ");
			AppendPointInner(builder, p);
        }

        private void AppendPointInner(StringBuilder builder, CoordinateInfo point)
        {
			if (point == null)
            {
                builder.Append("EMPTY");
                return;
            }

            builder.Append("(");
			AppendCoordinate(builder, point);
            builder.Append(")");
        }

        private void AppendLineString(StringBuilder builder, object lineString)
        {
            var ls = _shapeConverter.FromLineString(lineString);
            builder.Append("LINESTRING");
            AppendDimensions(builder, GetDimensions(ls));
            builder.Append(" ");
			AppendLineStringInner(builder, ls);
        }

		//private void AppendLinearRing(StringBuilder builder, object linearRing)
		//{
		//	builder.Append("LINEARRING");
		//	AppendDimensions(builder, linearRing);
		//	builder.Append(" ");
		//	AppendLineStringInner(builder, linearRing.Coordinates);
		//}

        private void AppendLineStringInner(StringBuilder builder, CoordinateInfo[] lineString)
        {
			if (lineString.Length == 0)
            {
                builder.Append("EMPTY");
                return;
            }

            builder.Append("(");
			AppendCoordinates(builder, lineString);
            builder.Append(")");
        }

        private void AppendPolygon(StringBuilder builder, object polygon)
        {
            var p = _shapeConverter.FromPolygon(polygon);
            builder.Append("POLYGON");
            AppendDimensions(builder, GetDimensions(p));
            builder.Append(" ");
			AppendPolygonInner(builder, p);
        }

		//private void AppendTriangle(StringBuilder builder, object polygon)
		//{
		//	builder.Append("TRIANGLE");
		//	AppendDimensions(builder, polygon);
		//	builder.Append(" ");
		//	AppendPolygonInner(builder, polygon);
		//}

        private void AppendPolygonInner(StringBuilder builder, CoordinateInfo[][] polygon)
        {
			if (polygon.Length == 0)
            {
                builder.Append("EMPTY");
                return;
            }

            builder.Append("(");
			AppendLineStringInner(builder, polygon[0]);
			for (var i = 1; i < polygon.Length; i++)
            {
                builder.Append(", ");
				AppendLineStringInner(builder, polygon[i]);
            }
            builder.Append(")");
        }

        private void AppendMultiPoint(StringBuilder builder, object multiPoint)
		{
			var geometries = _shapeConverter.FromMultiPoint(multiPoint);

            builder.Append("MULTIPOINT");
			if (geometries.Length == 0)
            {
                builder.Append(" EMPTY");
                return;
            }

            AppendDimensions(builder, GetDimensions(geometries));
            builder.Append(" (");
			for (var i = 0; i < geometries.Length; i++)
            {
                if (i > 0)
                    builder.Append(", ");
				AppendPointInner(builder, geometries[i]);
            }
            builder.Append(")");
        }

        private void AppendMultiLineString(StringBuilder builder, object multiLineString)
		{
			var geometries = _shapeConverter.FromMultiLineString(multiLineString);

            builder.Append("MULTILINESTRING");
			if (geometries.Length == 0)
            {
                builder.Append(" EMPTY");
                return;
            }

            AppendDimensions(builder, GetDimensions(geometries));
			builder.Append(" (");

			for (var i = 0; i < geometries.Length; i++)
            {
                if (i > 0)
                    builder.Append(", ");
				AppendLineStringInner(builder, geometries[i]);
            }
            builder.Append(")");
        }

        private void AppendMultiPolygon(StringBuilder builder, object multiPolygon)
		{
			var geometries = _shapeConverter.FromMultiPolygon(multiPolygon);

            builder.Append("MULTIPOLYGON");
			if (geometries.Length == 0)
            {
                builder.Append(" EMPTY");
                return;
            }

            AppendDimensions(builder, GetDimensions(geometries));
            builder.Append(" (");

			for (var i = 0; i < geometries.Length; i++)
            {
                if (i > 0)
                    builder.Append(", ");
				AppendPolygonInner(builder, geometries[i]);
            }
            builder.Append(")");
        }

        private void AppendGeometryCollection(StringBuilder builder, object geometryCollection)
		{
			var geometries = _shapeConverter.FromGeometryCollection(geometryCollection);

            builder.Append("GEOMETRYCOLLECTION");
			if (geometries.Length == 0)
            {
                builder.Append(" EMPTY");
                return;
            }

            //AppendDimensions(builder, geometryCollection);
            builder.Append(" (");

			for (var i = 0; i < geometries.Length; i++)
            {
                if (i > 0)
                    builder.Append(", ");
				AppendGeometry(builder, geometries[i]);
            }
            builder.Append(")");
        }

        private void AppendDimensions(StringBuilder builder, Dimensions dimensions)
        {
            if (_settings.DimensionFlag && _settings.MaxDimesions > 2)
            {
                if (dimensions.HasFlag(Dimensions.Z) || dimensions.HasFlag(Dimensions.M))
                    builder.Append(" ");

                if (dimensions.HasFlag(Dimensions.Z) && _settings.MaxDimesions > 2)
                    builder.Append("Z");

                if (dimensions.HasFlag(Dimensions.M) && _settings.MaxDimesions > 3)
                    builder.Append("M");
            }
        }

        private void AppendCoordinates(StringBuilder builder, CoordinateInfo[] coordinates)
        {
            for (var i = 0; i < coordinates.Length; i++)
            {
                if (i > 0)
                    builder.Append(", ");
                AppendCoordinate(builder, coordinates[i]);
            }
        }

        private void AppendCoordinate(StringBuilder builder, CoordinateInfo coordinate)
        {
            builder.Append(coordinate.X.ToString(CultureInfo.InvariantCulture));
            builder.Append(" ");
			builder.Append(coordinate.Y.ToString(CultureInfo.InvariantCulture));

			if (coordinate.Z.HasValue && _settings.MaxDimesions > 2)
			{
				builder.Append(" ");
				builder.Append(coordinate.Z.Value.ToString(CultureInfo.InvariantCulture));
			}

			if (coordinate.M.HasValue && _settings.MaxDimesions > 3)
			{
				builder.Append(" ");
				builder.Append(coordinate.M.Value.ToString(CultureInfo.InvariantCulture));
			}
        }

        private Dimensions GetDimensions(CoordinateInfo[][][] coordinates)
        {
            return coordinates.Aggregate(Dimensions.XY, (current, coords) => current | GetDimensions(coords));
        }

        private Dimensions GetDimensions(CoordinateInfo[][] coordinates)
        {
            return coordinates.Aggregate(Dimensions.XY, (current, coords) => current | GetDimensions(coords));
        }

        private Dimensions GetDimensions(CoordinateInfo[] coordinates)
        {
            return coordinates.Aggregate(Dimensions.XY, (current, coordinate) => current | coordinate.GetDimensions());
        }
    }
}
