using System;
using System.Collections.Generic;
using System.Globalization;
using System.Runtime.Serialization;

namespace Raven.Client.Spatial
{
    internal class WktReader
	{
        private readonly WktTokenizer _wktTokenizer = new WktTokenizer();
		private readonly IShapeConverter _shapeConverter;

		public WktReader(IShapeConverter shapeConverter)
		{
			_shapeConverter = shapeConverter;
		}

        public object Read(string wkt)
        {
            if (wkt == null)
                throw new ArgumentNullException("wkt");

            var tokens = new WktTokenQueue(_wktTokenizer.Tokenize(wkt));
            return ParseGeometry(tokens);
        }

        private object ParseGeometry(WktTokenQueue tokens)
        {
            if (tokens.Count == 0)
                return null;

            var token = tokens.Peek();

            if (token.Type == WktTokenType.String)
            {
                var value = token.Value.ToUpperInvariant();
                if (value == "POINT")
                    return ParsePoint(tokens);
                if (value == "LINESTRING")
                    return ParseLineString(tokens);
                if (value == "LINEARRING")
                    return ParseLinearRing(tokens);
                if (value == "POLYGON")
                    return ParsePolygon(tokens);
                if (value == "TRIANGLE")
                    return ParseTriangle(tokens);
                if (value == "MULTIPOINT")
                    return ParseMultiPoint(tokens);
                if (value == "MULTILINESTRING")
                    return ParseMultiLineString(tokens);
                if (value == "MULTIPOLYGON")
                    return ParseMultiPolygon(tokens);
                if (value == "GEOMETRYCOLLECTION")
                    return ParseGeometryCollection(tokens);
            }
            throw new SerializationException("WKT type '" + token.Value + "' not supported.");
        }

        private object ParsePoint(WktTokenQueue tokens)
        {
            tokens.Dequeue("POINT");
            var dimensions = ParseDimensions(tokens);

            if (tokens.NextTokenIs("EMPTY"))
            {
                tokens.Dequeue();
				return _shapeConverter.ToPoint(null);
            }

            tokens.Dequeue(WktTokenType.LeftParenthesis);
            var coordinate = ParseCoordinate(tokens, dimensions);
            tokens.Dequeue(WktTokenType.RightParenthesis);
			return _shapeConverter.ToPoint(coordinate);
        }

        private object ParseLineString(WktTokenQueue tokens)
        {
            tokens.Dequeue("LINESTRING");
            var dimensions = ParseDimensions(tokens);
            return ParseLineStringInner(tokens, dimensions);
        }

        private object ParseLineStringInner(WktTokenQueue tokens, Dimensions dimensions)
        {
            var coords = ParseCoordinateSequence(tokens, dimensions);
            return _shapeConverter.ToLineString(coords ?? new CoordinateInfo[0]);
        }

        private object ParseLinearRing(WktTokenQueue tokens)
        {
            tokens.Dequeue("LINEARRING");
            var dimensions = ParseDimensions(tokens);
            var coords = ParseCoordinateSequence(tokens, dimensions);
            return _shapeConverter.ToLinearRing(coords ?? new CoordinateInfo[0]);
        }

        private object ParsePolygon(WktTokenQueue tokens)
        {
            tokens.Dequeue("POLYGON");
            var dimensions = ParseDimensions(tokens);
            return ParsePolygonInner(tokens, dimensions);
        }

        private object ParseTriangle(WktTokenQueue tokens)
        {
            tokens.Dequeue("TRIANGLE");
            var dimensions = ParseDimensions(tokens);
            return ParsePolygonInner(tokens, dimensions);
        }

        private object ParsePolygonInner(WktTokenQueue tokens, Dimensions dimensions)
        {
            if (tokens.NextTokenIs("EMPTY"))
            {
                tokens.Dequeue();
				return _shapeConverter.FromPolygon(new CoordinateInfo[0][][]);
            }

            var linestrings = ParseLineStrings(tokens, dimensions);

	        return _shapeConverter.ToPolygon(linestrings);
        }

		private CoordinateInfo[][] ParseLineStrings(WktTokenQueue tokens, Dimensions dimensions)
        {
            tokens.Dequeue(WktTokenType.LeftParenthesis);
			var lineStrings = new List<CoordinateInfo[]> { ParseCoordinateSequence(tokens, dimensions) };

            while (tokens.NextTokenIs(WktTokenType.Comma))
            {
                tokens.Dequeue();
				lineStrings.Add(ParseCoordinateSequence(tokens, dimensions));
            }

            tokens.Dequeue(WktTokenType.RightParenthesis);
            return lineStrings.ToArray();
        }

        private object ParseMultiPoint(WktTokenQueue tokens)
        {
            tokens.Dequeue("MULTIPOINT");
            var dimensions = ParseDimensions(tokens);

            if (tokens.NextTokenIs("EMPTY"))
            {
                tokens.Dequeue();
                return _shapeConverter.ToMultiPoint(new CoordinateInfo[0]);
            }

            tokens.Dequeue(WktTokenType.LeftParenthesis);


            var points = new List<CoordinateInfo> { ParseMultiPointCoordinate(tokens, dimensions) };
            while (tokens.NextTokenIs(WktTokenType.Comma))
            {
                tokens.Dequeue();
                points.Add(ParseMultiPointCoordinate(tokens, dimensions));
            }

            tokens.Dequeue(WktTokenType.RightParenthesis);

            return _shapeConverter.ToMultiPoint(points.ToArray());
        }

        private CoordinateInfo ParseMultiPointCoordinate(WktTokenQueue tokens, Dimensions dimensions)
        {
            if (tokens.NextTokenIs("EMPTY"))
            {
                tokens.Dequeue();
                return null;
            }

            var parenthesis = false;

            if (tokens.NextTokenIs(WktTokenType.LeftParenthesis))
            {
                tokens.Dequeue(WktTokenType.LeftParenthesis);
                parenthesis = true;
            }
            var coordinate = ParseCoordinate(tokens, dimensions);
            if (parenthesis && tokens.NextTokenIs(WktTokenType.RightParenthesis))
                tokens.Dequeue(WktTokenType.RightParenthesis);
			return coordinate;
        }

        private object ParseMultiLineString(WktTokenQueue tokens)
        {
            tokens.Dequeue("multilinestring");
            var dimensions = ParseDimensions(tokens);

            if (tokens.NextTokenIs("EMPTY"))
            {
                tokens.Dequeue();
                return _shapeConverter.ToMultiLineString(new CoordinateInfo[0][]);
            }

            var lineStrings = ParseLineStrings(tokens, dimensions);

            return _shapeConverter.ToMultiLineString(lineStrings);
        }

        private object ParseMultiPolygon(WktTokenQueue tokens)
        {
            tokens.Dequeue("MULTIPOLYGON");
            var dimensions = ParseDimensions(tokens);

            if (tokens.NextTokenIs("EMPTY"))
            {
                tokens.Dequeue();
                return _shapeConverter.ToMultiPolygon(new CoordinateInfo[0][][]);
            }

            tokens.Dequeue(WktTokenType.LeftParenthesis);
			var polygons = new List<CoordinateInfo[][]> { ParseLineStrings(tokens, dimensions) };
            while (tokens.NextTokenIs(WktTokenType.Comma))
            {
                tokens.Dequeue();
				polygons.Add(ParseLineStrings(tokens, dimensions));
            }
            tokens.Dequeue(WktTokenType.RightParenthesis);

			return _shapeConverter.ToMultiPolygon(polygons.ToArray());
        }

        private object ParseGeometryCollection(WktTokenQueue tokens)
        {
            tokens.Dequeue("GEOMETRYCOLLECTION");

            ParseDimensions(tokens);

            if (tokens.NextTokenIs("EMPTY"))
            {
                tokens.Dequeue();
				return _shapeConverter.ToGeometryCollection(new object[0]);
            }

            tokens.Dequeue(WktTokenType.LeftParenthesis);

            var geometries = new List<object>();
            geometries.Add(ParseGeometry(tokens));

            while (tokens.NextTokenIs(WktTokenType.Comma))
            {
                tokens.Dequeue();
                geometries.Add(ParseGeometry(tokens));
            }

            tokens.Dequeue(WktTokenType.RightParenthesis);

            return _shapeConverter.ToGeometryCollection(geometries.ToArray());
        }

        private CoordinateInfo ParseCoordinate(WktTokenQueue tokens, Dimensions dimensions)
        {
            var token = tokens.Dequeue(WktTokenType.Number);
            var x = double.Parse(token.Value, CultureInfo.InvariantCulture);

            token = tokens.Dequeue(WktTokenType.Number);
            var y = double.Parse(token.Value, CultureInfo.InvariantCulture);

            var z = double.NaN;
            var m = double.NaN;

            var optional = ParseOptionalOrdinates(tokens);

            if (optional.Count > 0)
            {
                if (dimensions.HasFlag(Dimensions.M) && !dimensions.HasFlag(Dimensions.Z))
                {
                    m = optional[0];
                }
                else
                {
                    z = optional[0];
                    if (optional.Count > 1)
                        m = optional[1];
                }
            }

            if (!double.IsNaN(z) && !double.IsNaN(m))
                return new CoordinateInfo { X = x, Y = y, Z = z, M = m };
            if (!double.IsNaN(z))
                return new CoordinateInfo { X = x, Y = y, Z = z };
            if (!double.IsNaN(m))
                return new CoordinateInfo { X = x, Y = y, M = m };
            return new CoordinateInfo { X = x, Y = y };
        }

        private List<double> ParseOptionalOrdinates(WktTokenQueue tokens)
        {
            var attempt = true;
            var doubles = new List<double>();

            while (attempt)
            {
                if (tokens.NextTokenIs(WktTokenType.Number))
                {
                    var token = tokens.Dequeue(WktTokenType.Number);
                    doubles.Add(double.Parse(token.Value, CultureInfo.InvariantCulture));
                }
                else if (tokens.NextTokenIs(double.NaN.ToString(CultureInfo.InvariantCulture)))
                {
                    //TODO: Review this
                    tokens.Dequeue(WktTokenType.String);
                    doubles.Add(double.NaN);
                }
                else
                {
                    attempt = false;
                }
            }
            return doubles;
        }

        private CoordinateInfo[] ParseCoordinateSequence(WktTokenQueue tokens, Dimensions dimensions)
        {
            if (tokens.NextTokenIs("EMPTY"))
            {
                tokens.Dequeue();
                return new CoordinateInfo[0];
            }

            tokens.Dequeue(WktTokenType.LeftParenthesis);

            var coordinates = new List<CoordinateInfo> { ParseCoordinate(tokens, dimensions) };
            while (tokens.NextTokenIs(WktTokenType.Comma))
            {
                tokens.Dequeue();
                coordinates.Add(ParseCoordinate(tokens, dimensions));
            }

            tokens.Dequeue(WktTokenType.RightParenthesis);

			return coordinates.ToArray();
        }

        private Dimensions ParseDimensions(WktTokenQueue tokens)
        {
            var result = Dimensions.XY;
            var token = tokens.Peek();
            if (token.Type == WktTokenType.String)
            {
                var value = token.Value.ToUpperInvariant();
                if (value == "Z")
                {
                    tokens.Dequeue();
                    result |= Dimensions.Z;
                }
                if (value == "M")
                {
                    tokens.Dequeue();
                    result |= Dimensions.M;
                }
                if (value == "ZM")
                {
                    tokens.Dequeue();
                    result |= Dimensions.Z;
                    result |= Dimensions.M;
                }
            }
            return result;
        }
    }
}
