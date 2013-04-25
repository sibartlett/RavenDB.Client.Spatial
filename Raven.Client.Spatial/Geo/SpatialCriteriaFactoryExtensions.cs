using Geo.Abstractions.Interfaces;
using Raven.Client.Spatial;

namespace Raven.Client
{
	public static class SpatialCriteriaFactoryExtensions
	{
		public static SpatialCriteria WithinRadiusOf(this SpatialCriteriaFactory @this,
													double radius,
													IPosition position)
		{
			var coordinate = position.GetCoordinate();
			return @this.WithinRadiusOf(radius, coordinate.Longitude, coordinate.Latitude);
		}
	}
}
