using System;
using System.Collections.Generic;
using System.Linq;
using Geo.Abstractions.Interfaces;
using Geo.Geometries;
using Geo.Gps;
using Raven.Imports.Newtonsoft.Json.Linq;
using Raven.Json.Linq;

namespace Raven.Client.Spatial.Geo
{
	internal class GeoJsonReader : Spatial.GeoJsonReader
	{
		public GeoJsonReader(IShapeConverter shapeConverter) : base(shapeConverter)
		{
		}

		protected override bool TryRead(RavenJObject json, out object result)
		{
			object res;
			if (base.TryRead(json, out res))
			{
				var lineString = res as LineString;
				if (lineString != null)
				{
					var geo = json["__geo"] as RavenJObject;
					if (geo != null)
					{
						var val = geo["type"] as RavenJValue;
						if (val != null)
						{
							switch (val.Value as string)
							{
								case "Route":
									return TryReadRoute(geo, lineString, out result);
								case "Track":
									return TryReadTrack(geo, lineString, out result);
									break;
							}
						}
					}
				}
				result = res;
				return true;
			}

			result = null;
			return false;
		}

		private static bool TryReadRoute(RavenJObject geo, LineString lineString, out object result)
		{
			var route = new Route();
			route.Coordinates.AddRange(lineString.Coordinates);
			ReadMetadata(geo, route.Metadata);
			result = route;
			return true;
		}

		private static bool TryReadTrack(RavenJObject geo, LineString lineString, out object result)
		{
			var times = geo["times"] as RavenJArray;
			if (times != null)
			{
				var times2 = times.OfType<RavenJArray>().ToList();
				var track = new Track();
				ReadMetadata(geo, track.Metadata);
				var fixCount = 0;
				foreach (var ti in times2)
				{
					var tt = ti.OfType<RavenJValue>().Where(x => x.Type == JTokenType.String).Select(x => DateTime.Parse((string)x.Value)).ToList();

					var segment = new TrackSegment();
					for (var i = 0; i < tt.Count; i++)
					{
						if (fixCount + 1 > lineString.Coordinates.Count)
							break;

						var coord2D = lineString.Coordinates[fixCount];
						var coord3D = coord2D as Is3D;
						if (coord3D == null)
							segment.Fixes.Add(new Fix(coord2D.Latitude, coord2D.Longitude, tt[i]));
						else
							segment.Fixes.Add(new Fix(coord2D.Latitude, coord2D.Longitude,
													  coord3D.Elevation, tt[i]));
						fixCount++;
					}
					track.Segments.Add(segment);
				}

				if (fixCount == lineString.Coordinates.Count)
				{
					result = track;
					return true;
				}
			}
			result = default(object);
			return false;
		}

		private static void ReadMetadata(RavenJObject geo, Dictionary<string, string> metadata)
		{
			var mda = geo["metadata"] as RavenJObject;
			if (mda != null)
				foreach (var a in mda)
				{
					var v = a.Value as RavenJValue;
					if (v != null && v.Type == JTokenType.String)
						metadata[a.Key] = v.Value<string>();
				}
		}
	}
}
