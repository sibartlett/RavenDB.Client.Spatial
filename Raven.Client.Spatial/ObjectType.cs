namespace Raven.Client.Spatial
{
	internal enum ObjectType
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