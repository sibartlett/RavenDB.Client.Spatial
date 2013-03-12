using System.Collections.Generic;

namespace Raven.Client.Spatial
{
    internal interface IShapeConverter
	{
		bool IsValid(object obj);
		GeoJsonObjectType GetGeoJsonObjectType(object obj);
		WktObjectType GetWktObjectType(object obj);

		object ToPoint(CoordinateInfo coordinates);
		object ToLineString(CoordinateInfo[] coordinates);
		object ToLinearRing(CoordinateInfo[] coordinates);
		object ToPolygon(CoordinateInfo[][] coordinates);
		object ToMultiPoint(CoordinateInfo[] coordinates);
		object ToMultiLineString(CoordinateInfo[][] coordinates);
		object ToMultiPolygon(CoordinateInfo[][][] coordinates);
		object ToGeometryCollection(object[] geometries);
		object ToFeature(object geometry, object id, Dictionary<string, object> properties);
		object ToFeatureCollection(object[] features);

		CoordinateInfo FromPoint(object point);
		CoordinateInfo[] FromLineString(object lineString);
		CoordinateInfo[] FromLinearRing(object lineString);
		CoordinateInfo[][] FromPolygon(object polygon);
		CoordinateInfo[] FromMultiPoint(object multiPoint);
		CoordinateInfo[][] FromMultiLineString(object multiLineString);
		CoordinateInfo[][][] FromMultiPolygon(object multiPolygon);
		object[] FromGeometryCollection(object geometryCollection);
		object FromFeature(object feature, out object id, out Dictionary<string, object> properties);
		object[] FromFeatureCollection(object featureCollection);
	}
}
