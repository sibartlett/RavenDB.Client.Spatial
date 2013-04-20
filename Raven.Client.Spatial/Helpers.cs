using System;

namespace Raven.Client.Spatial
{
	internal static class Helpers
	{
		public static double ToRadians(this double degrees)
		{
			return degrees * Math.PI / 180.0;
		}

		public static double ToDegrees(this double radians)
		{
			return radians * 180 / Math.PI;
		}
	}

	internal class Constants
	{
		public const double NauticalMile = 1852d;
		public const double EarthMeanRadius = 6371008.7714d;
	}
}
