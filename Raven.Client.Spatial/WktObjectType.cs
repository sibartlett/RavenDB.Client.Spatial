namespace Raven.Client.Spatial
{
	internal enum WktObjectType
	{
		Point,
		MultiPoint,
		LineString,
		MultiLineString,
		Polygon,
		MultiPolygon,
		GeometryCollection,
		Feature,
		FeatureCollection,

		Envelope,
		Circle
	}
}