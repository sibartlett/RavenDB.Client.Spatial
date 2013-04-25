using GeoAPI.Geometries;
using Raven.Client.Spatial;

namespace Raven.Client
{
	public static class SpatialCriteriaFactoryExtensions
	{
		public static SpatialCriteria WithinRadiusOf(this SpatialCriteriaFactory @this,
													double radius,
													IPoint point)
		{
			return @this.WithinRadiusOf(radius, point.Coordinate);
		}

		public static SpatialCriteria WithinRadiusOf(this SpatialCriteriaFactory @this,
													double radius,
													Coordinate coordinate)
		{
			return @this.WithinRadiusOf(radius, coordinate.X, coordinate.Y);
		}
	}
}
