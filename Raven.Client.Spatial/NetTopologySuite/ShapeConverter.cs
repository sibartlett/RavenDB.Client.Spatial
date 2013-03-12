using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using GeoAPI.Geometries;
using NetTopologySuite.Features;
using NetTopologySuite.Geometries;

namespace Raven.Client.Spatial.NetTopologySuite
{
	internal class ShapeConverter : IShapeConverter
	{
		private readonly IGeometryFactory _geometryFactory;

		public ShapeConverter(IGeometryFactory geometryFactory)
		{
			_geometryFactory = geometryFactory;
		}

		public bool IsValid(object obj)
		{
			return obj is IPoint
				|| obj is ILineString
				|| obj is IPolygon
				|| obj is IMultiPoint
				|| obj is IMultiLineString
				|| obj is IMultiPolygon
				|| obj is IGeometryCollection
				|| obj is Feature
#if !SILVERLIGHT
				|| obj is FeatureCollection
#endif
				;
		}

		public GeoJsonObjectType GetGeoJsonObjectType(object obj)
		{
			if (obj is IPoint)
				return GeoJsonObjectType.Point;
			if (obj is ILineString)
				return GeoJsonObjectType.LineString;
			if (obj is IPolygon)
				return GeoJsonObjectType.Polygon;
			if (obj is IMultiPoint)
				return GeoJsonObjectType.MultiPoint;
			if (obj is IMultiLineString)
				return GeoJsonObjectType.MultiLineString;
			if (obj is IMultiPolygon)
				return GeoJsonObjectType.MultiPolygon;
			if (obj is IGeometryCollection)
				return GeoJsonObjectType.GeometryCollection;
			if (obj is Feature)
				return GeoJsonObjectType.Feature;
#if !SILVERLIGHT
			if (obj is FeatureCollection)
				return GeoJsonObjectType.FeatureCollection;
#endif

			throw new ArgumentException("geom");
		}

		public WktObjectType GetWktObjectType(object obj)
		{
			if (obj is IPoint)
				return WktObjectType.Point;
			if (obj is ILineString)
				return WktObjectType.LineString;
			if (obj is IPolygon)
				return WktObjectType.Polygon;
			if (obj is IMultiPoint)
				return WktObjectType.MultiPoint;
			if (obj is IMultiLineString)
				return WktObjectType.MultiLineString;
			if (obj is IMultiPolygon)
				return WktObjectType.MultiPolygon;
			if (obj is IGeometryCollection)
				return WktObjectType.GeometryCollection;

			throw new ArgumentException("obj");
		}

	    private Coordinate MakeCoordinate(CoordinateInfo coordinate)
	    {
            if (coordinate.Z.HasValue)
                return new Coordinate(coordinate.X, coordinate.Y, coordinate.Z.Value);
            return new Coordinate(coordinate.X, coordinate.Y);
	    }

        public object ToPoint(CoordinateInfo coordinates)
        {
            if (coordinates == null)
                return Point.Empty;
			return _geometryFactory.CreatePoint(MakeCoordinate(coordinates));
		}

        public object ToLineString(CoordinateInfo[] coordinates)
        {
            if (coordinates.Length == 0)
                return LineString.Empty;
			return _geometryFactory.CreateLineString(coordinates.Select(MakeCoordinate).ToArray());
		}

        public object ToLinearRing(CoordinateInfo[] coordinates)
        {
            if (coordinates.Length == 0)
                return LinearRing.Empty;
			return _geometryFactory.CreateLinearRing(coordinates.Select(MakeCoordinate).ToArray());
		}

        public object ToPolygon(CoordinateInfo[][] coordinates)
        {
            if (coordinates.Length == 0)
                return Polygon.Empty;
			return _geometryFactory.CreatePolygon(
				_geometryFactory.CreateLinearRing(coordinates.First().Select(MakeCoordinate).ToArray()),
				coordinates.Skip(1).Select(x => _geometryFactory.CreateLinearRing(x.Select(MakeCoordinate).ToArray())).ToArray()
				);
		}

        public object ToMultiPoint(CoordinateInfo[] coordinates)
        {
            if (coordinates.Length == 0)
                return MultiPoint.Empty;
			return _geometryFactory.CreateMultiPoint(coordinates.Select(ToPoint).Cast<IPoint>().ToArray());
		}

        public object ToMultiLineString(CoordinateInfo[][] coordinates)
        {
            if (coordinates.Length == 0)
                return MultiLineString.Empty;
			return _geometryFactory.CreateMultiLineString(coordinates.Select(ToLineString).Cast<ILineString>().ToArray());
		}

        public object ToMultiPolygon(CoordinateInfo[][][] coordinates)
        {
            if (coordinates.Length == 0)
                return MultiPolygon.Empty;
			return _geometryFactory.CreateMultiPolygon(coordinates.Select(ToPolygon).Cast<IPolygon>().ToArray());
		}

		public object ToGeometryCollection(object[] geometries)
        {
            if (geometries.Length == 0)
                return GeometryCollection.Empty;
			return _geometryFactory.CreateGeometryCollection(geometries.Cast<IGeometry>().ToArray());
		}

		public object ToFeature(object geometry, object id, Dictionary<string, object> properties)
		{
			AttributesTable props = null;
			if (properties != null)
			{
				props = new AttributesTable();
				foreach (var property in properties)
					props.AddAttribute(property.Key, property.Value);
			}

			return new Feature((IGeometry)geometry, props);
		}

		public object ToFeatureCollection(object[] features)
		{
#if SILVERLIGHT
			throw new NotImplementedException();
#else
			return new FeatureCollection(new Collection<Feature>(features.Cast<Feature>().ToList()));
#endif
		}

	    private CoordinateInfo MakeCoordinate(Coordinate coordinate)
	    {
	        return new CoordinateInfo
	            {
	                X = coordinate.X,
	                Y = coordinate.Y,
                    Z = double.IsNaN(coordinate.Z) ? (double?)null : coordinate.Z
	            };
	    }

        public CoordinateInfo FromPoint(object point)
        {
            if (((IPoint)point).IsEmpty)
                return null;
			return MakeCoordinate(((IPoint)point).Coordinate);
		}

        public CoordinateInfo[] FromLineString(object lineString)
        {
            if (((ILineString)lineString).IsEmpty)
                return new CoordinateInfo[0];
			return ((ILineString) lineString).Coordinates.Select(MakeCoordinate).ToArray();
		}

        public CoordinateInfo[] FromLinearRing(object lineString)
        {
            if (((ILinearRing)lineString).IsEmpty)
                return new CoordinateInfo[0];
			return ((ILinearRing)lineString).Coordinates.Select(MakeCoordinate).ToArray();
		}

        public CoordinateInfo[][] FromPolygon(object polygon)
        {
            if (((IPolygon)polygon).IsEmpty)
                return new CoordinateInfo[0][];

			var p = (IPolygon) polygon;
            var list = new List<CoordinateInfo[]>();
			list.Add(p.Shell.Coordinates.Select(MakeCoordinate).ToArray());
			list.AddRange(p.Holes.Select(x => x.Coordinates.Select(MakeCoordinate).ToArray()).ToArray());
			return list.ToArray();
		}

        public CoordinateInfo[] FromMultiPoint(object multiPoint)
        {
            if (((IMultiPoint)multiPoint).IsEmpty)
                return new CoordinateInfo[0];

			return ((IMultiPoint) multiPoint).Geometries.Cast<IPoint>().Select(FromPoint).ToArray();
		}

        public CoordinateInfo[][] FromMultiLineString(object multiLineString)
        {
            if (((IMultiLineString)multiLineString).IsEmpty)
                return new CoordinateInfo[0][];

			return ((IMultiLineString) multiLineString).Geometries.Cast<ILineString>().Select(FromLineString).ToArray();
		}

        public CoordinateInfo[][][] FromMultiPolygon(object multiPolygon)
        {
            if (((IMultiPolygon)multiPolygon).IsEmpty)
                return new CoordinateInfo[0][][];

			return ((IMultiPolygon)multiPolygon).Geometries.Cast<IPolygon>().Select(FromPolygon).ToArray();
		}

		public object[] FromGeometryCollection(object geometryCollection)
		{
			return ((IGeometryCollection) geometryCollection).Geometries.Cast<object>().ToArray();
		}

		public object FromFeature(object feature, out object id, out Dictionary<string, object> properties)
		{
			var feat = (Feature) feature;
			id = default(object);
			if (feat.Attributes == null)
				properties = default(Dictionary<string, object>);
			else
				properties = Enumerable.ToDictionary(feat.Attributes.GetNames(), x => x, x => feat.Attributes[x]);
			return feat.Geometry;
		}

		public object[] FromFeatureCollection(object featureCollection)
		{
#if SILVERLIGHT
			throw new NotImplementedException();
#else
			return ((FeatureCollection) featureCollection).Features.Cast<object>().ToArray();
#endif
		}
	}
}