using System;
using System.Collections.Generic;
using System.Linq;
using Raven.Imports.Newtonsoft.Json;
using Raven.Imports.Newtonsoft.Json.Linq;
using Raven.Json.Linq;

namespace Raven.Client.Spatial
{
	internal class GeoJsonReader
	{
		private readonly IShapeConverter _shapeConverter;

		public GeoJsonReader(IShapeConverter shapeConverter)
		{
			_shapeConverter = shapeConverter;
		}

		public object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
		{
			if (reader.TokenType == JsonToken.Null)
				return null;

			var json = RavenJToken.ReadFrom(reader);
			object result;
			TryRead(json as RavenJObject, out result); // Throw exception?
			return result;
		}

		private bool TryRead(RavenJObject json, out object result)
		{
			if (TryParseGeometry(json, out result))
				return true;
			if (TryParseFeature(json, out result))
				return true;
			if (TryParseFeatureCollection(json, out result))
				return true;

			result = null;
			return false;
		}

		private bool TryParseTypeString(RavenJObject obj, out string result)
		{
			RavenJToken type = null;
			if (obj != null)
				obj.TryGetValue("type", out type);

			var value = type as RavenJValue;
			if (value != null)
				result = value.Value as string;
			else
				result = null;

			return type != null;
		}

		private bool TryParseFeatureCollection(RavenJObject obj, out object result)
		{
			result = null;
			string typeString;
			if (TryParseTypeString(obj, out typeString) && typeString.ToLowerInvariant() == "featurecollection")
			{
				RavenJToken feats;
				if (obj.TryGetValue("features", out feats))
				{
					var features = feats as RavenJArray;
					if (features != null)
					{
						var temp = new object[features.Length];
						for (var index = 0; index < features.Length; index++)
						{
							var geometry = features[index];
							if (!TryParseFeature((RavenJObject)geometry, out temp[index]))
								return false;
						}
						result = _shapeConverter.ToFeatureCollection(temp);
						return true;
					}
				}
			}
			return false;
		}

		private bool TryParseFeature(RavenJObject obj, out object result)
		{
			string typeString;
			if (TryParseTypeString(obj, out typeString) && typeString.ToLowerInvariant() == "feature")
			{
				RavenJToken geometry;
				object geo;
				if (obj.TryGetValue("geometry", out geometry) && TryParseGeometry((RavenJObject)geometry, out geo))
				{
					RavenJToken prop;
					Dictionary<string, object> pr = null;
					if (obj.TryGetValue("properties", out prop) && prop is RavenJObject)
					{
						var props = (RavenJObject)prop;
						if (props.Count > 0)
						{
							pr = Enumerable.ToDictionary(props, x => x.Key, x => SantizeRavenJObjects(x.Value));
						}
					}

					object id = null;
					RavenJToken idToken;
					if (obj.TryGetValue("id", out idToken))
					{
						id = SantizeRavenJObjects(idToken);
					}

					result = _shapeConverter.ToFeature(geo, id, pr);
					return true;
				}
			}
			result = null;
			return false;
		}

		private bool TryParseGeometry(RavenJObject obj, out object result)
		{
			result = null;
			string typeString;
			if (!TryParseTypeString(obj, out typeString))
				return false;

			typeString = typeString.ToLowerInvariant();

			switch (typeString)
			{
				case "point":
					return TryParsePoint(obj, out result);
				case "linestring":
					return TryParseLineString(obj, out result);
				case "polygon":
					return TryParsePolygon(obj, out result);
				case "multipoint":
					return TryParseMultiPoint(obj, out result);
				case "multilinestring":
					return TryParseMultiLineString(obj, out result);
				case "multipolygon":
					return TryParseMultiPolygon(obj, out result);
				case "geometrycollection":
					return TryParseGeometryCollection(obj, out result);
				default:
					return false;
			}
		}

		private bool TryParsePoint(RavenJObject obj, out object result)
		{
			result = null;
			RavenJToken coord;
			if (obj.TryGetValue("coordinates", out coord))
			{
				var coordinates = coord as RavenJArray;

				if (coordinates == null || coordinates.Length < 2)
					return false;

                CoordinateInfo coordinate;
				if (TryParseCoordinate(coordinates, out coordinate))
				{
					result = _shapeConverter.ToPoint(coordinate);
					return true;
				}
			}
			return false;
		}


		private bool TryParseLineString(RavenJObject obj, out object result)
		{
			RavenJToken coord;
			if (obj.TryGetValue("coordinates", out coord))
			{
				var coordinates = coord as RavenJArray;
                CoordinateInfo[] co;
				if (coordinates != null && TryParseCoordinateArray(coordinates, out co))
				{
					result = _shapeConverter.ToLineString(co);
					return true;
				}
			}
			result = null;
			return false;
		}

		private bool TryParsePolygon(RavenJObject obj, out object result)
		{
			RavenJToken coord;
			if (obj.TryGetValue("coordinates", out coord))
			{
				var coordinates = coord as RavenJArray;

                CoordinateInfo[][] temp;
				if (coordinates != null && coordinates.Length > 0 && TryParseCoordinateArrayArray(coordinates, out temp))
				{
					result = _shapeConverter.ToPolygon(temp);
					return true;
				}
			}
			result = null;
			return false;
		}

		private bool TryParseMultiPoint(RavenJObject obj, out object result)
		{
			RavenJToken coord;
			if (obj.TryGetValue("coordinates", out coord))
			{
				var coordinates = coord as RavenJArray;
                CoordinateInfo[] co;
				if (coordinates != null && TryParseCoordinateArray(coordinates, out co))
				{
					result = _shapeConverter.ToMultiPoint(co);
					return true;
				}
			}
			result = null;
			return false;
		}

		private bool TryParseMultiLineString(RavenJObject obj, out object result)
		{
			RavenJToken coord;
			if (obj.TryGetValue("coordinates", out coord))
			{
				var coordinates = coord as RavenJArray;
                CoordinateInfo[][] co;
				if (coordinates != null && TryParseCoordinateArrayArray(coordinates, out co))
				{
					result = _shapeConverter.ToMultiLineString(co);
					return true;
				}
			}
			result = null;
			return false;
		}

		private bool TryParseMultiPolygon(RavenJObject obj, out object result)
		{
			RavenJToken coord;
			if (obj.TryGetValue("coordinates", out coord))
			{
				var coordinates = coord as RavenJArray;
                CoordinateInfo[][][] co;
				if (coordinates != null && TryParseCoordinateArrayArrayArray(coordinates, out co))
				{
					result = _shapeConverter.ToMultiPolygon(co);
					return true;
				}
			}
			result = null;
			return false;
		}

		private bool TryParseGeometryCollection(RavenJObject obj, out object result)
		{
			result = null;
			RavenJToken geom;
			if (obj.TryGetValue("geometries", out geom))
			{
				var geometries = geom as RavenJArray;

				if (geometries != null)
				{
					var temp = new object[geometries.Length];
					for (var index = 0; index < geometries.Length; index++)
					{
						var geometry = geometries[index];
						if (!TryParseGeometry((RavenJObject)geometry, out temp[index]))
							return false;
					}
					result = _shapeConverter.ToGeometryCollection(temp);
					return true;
				}
			}
			return false;
		}

        private bool TryParseCoordinate(RavenJArray coordinates, out CoordinateInfo result)
		{
			if (coordinates != null && coordinates.Length > 1 && coordinates.All(x => x is RavenJValue))
			{
				var vals = coordinates.Cast<RavenJValue>().ToList();
				if (vals.All(x => x.Type == JTokenType.Float || x.Type == JTokenType.Integer))
				{
				    result = new CoordinateInfo
				        {
                            X = Convert.ToDouble(vals[0].Value),
                            Y = Convert.ToDouble(vals[1].Value),
                            Z = vals.Count > 2 ? Convert.ToDouble(vals[2].Value) : (double?) null,
                            M = vals.Count > 3 ? Convert.ToDouble(vals[3].Value) : (double?) null
				        };
					return true;
				}
			}
			result = null;
			return false;
		}

        private bool TryParseCoordinateArray(RavenJArray coordinates, out CoordinateInfo[] result)
		{
			result = null;
			if (coordinates == null)
				return false;

			var valid = coordinates.All(x => x is RavenJArray);
			if (!valid)
				return false;

            var tempResult = new CoordinateInfo[coordinates.Length];
			for (var index = 0; index < coordinates.Length; index++)
			{
				if (!TryParseCoordinate((RavenJArray)coordinates[index], out tempResult[index]))
					return false;
			}
			result = tempResult;
			return true;
		}

        private bool TryParseCoordinateArrayArray(RavenJArray coordinates, out CoordinateInfo[][] result)
		{
			result = null;
			if (coordinates == null)
				return false;

			var valid = coordinates.All(x => x is RavenJArray);
			if (!valid)
				return false;

            var tempResult = new CoordinateInfo[coordinates.Length][];
			for (var index = 0; index < coordinates.Length; index++)
			{
				if (!TryParseCoordinateArray((RavenJArray)coordinates[index], out tempResult[index]))
					return false;
			}
			result = tempResult;
			return true;
		}

        private bool TryParseCoordinateArrayArrayArray(RavenJArray coordinates, out CoordinateInfo[][][] result)
		{
			result = null;
			if (coordinates == null)
				return false;

			var valid = coordinates.All(x => x is RavenJArray);
			if (!valid)
				return false;

            var tempResult = new CoordinateInfo[coordinates.Length][][];
			for (var index = 0; index < coordinates.Length; index++)
			{
				if (!TryParseCoordinateArrayArray((RavenJArray)coordinates[index], out tempResult[index]))
					return false;
			}
			result = tempResult;
			return true;
		}

		private object SantizeRavenJObjects(object obj)
		{
			var ravenJArray = obj as RavenJArray;
			if (ravenJArray != null)
				return ravenJArray.Select(SantizeRavenJObjects).ToArray();

			var ravenJObject = obj as RavenJObject;
			if (ravenJObject != null)
                return Enumerable.ToDictionary(ravenJObject, x => x.Key, x => SantizeRavenJObjects(x));

			return obj;
		}
	}
}