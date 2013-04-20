using System.Globalization;
using System.Text.RegularExpressions;

namespace Raven.Client.Spatial
{
	internal class Spatial4nReader
	{
		private readonly IShapeConverter _shapeConverter;
		private readonly WktReader _wktReader;

		public Spatial4nReader(IShapeConverter shapeConverter)
		{
			_shapeConverter = shapeConverter;
			_wktReader = new WktReader(shapeConverter);
		}

		public object Read(string value)
		{
			if (string.IsNullOrEmpty(value))
				return null;

			object result;

			if (TryReadCircle(value, out result))
				return result;

			if (TryReadGeoPoint(value, out result))
				return result;

			if (TryReadPoint(value, out result))
				return result;

			if (TryReadEnvelope(value, out result))
				return result;

			return _wktReader.Read(value);
		}

		protected virtual double ConvertCircleRadius(double radius)
		{
			return radius.ToDegrees() * Constants.EarthMeanRadius;
		}

		private bool TryReadCircle(string value, out object result)
		{
			if (!_shapeConverter.CanConvert(WktObjectType.Circle))
			{
				result = null;
				return false;
			}

			var match = Regex.Match(value,
						@"Circle \s* \( \s* ([+-]?(?:\d+\.?\d*|\d*\.?\d+)) \s+ ([+-]?(?:\d+\.?\d*|\d*\.?\d+)) \s+ d=([+-]?(?:\d+\.?\d*|\d*\.?\d+)) \s* \)",
						RegexOptions.IgnorePatternWhitespace | RegexOptions.IgnoreCase);
			if (match.Success)
			{
				result = _shapeConverter.ToCircle(new double[]
					{
						double.Parse(match.Groups[1].Value, CultureInfo.InvariantCulture),
						double.Parse(match.Groups[2].Value, CultureInfo.InvariantCulture),
						double.Parse(match.Groups[3].Value, CultureInfo.InvariantCulture)
					});
				return true;
			}
			result = null;
			return false;
		}

		private bool TryReadGeoPoint(string value, out object result)
		{
			var match = Regex.Match(value,
						@"^ \s* ([+-]?(?:\d+\.?\d*|\d*\.?\d+)) \s* , \s* ([+-]?(?:\d+\.?\d*|\d*\.?\d+)) \s* $",
						RegexOptions.IgnorePatternWhitespace | RegexOptions.IgnoreCase);
			if (match.Success)
			{
				result = _shapeConverter.ToPoint(new CoordinateInfo
					{
						X = double.Parse(match.Groups[2].Value, CultureInfo.InvariantCulture),
						Y = double.Parse(match.Groups[1].Value, CultureInfo.InvariantCulture)
					});
				return true;
			}
			result = null;
			return false;
		}

		private bool TryReadPoint(string value, out object result)
		{
			var match = Regex.Match(value,
						@"^ \s* (-?\d+.\d+?) \s+ (-?\d+.\d+?) \s* $",
						RegexOptions.IgnorePatternWhitespace | RegexOptions.IgnoreCase);
			if (match.Success)
			{
				result = _shapeConverter.ToPoint(new CoordinateInfo
				{
					X = double.Parse(match.Groups[1].Value, CultureInfo.InvariantCulture),
					Y = double.Parse(match.Groups[2].Value, CultureInfo.InvariantCulture)
				});
				return true;
			}
			result = null;
			return false;
		}

		private bool TryReadEnvelope(string value, out object result)
		{
			if (!_shapeConverter.CanConvert(WktObjectType.Envelope))
			{
				result = null;
				return false;
			}

			var match = Regex.Match(value,
						@"^ \s* (-?\d+.\d+?) \s+ (-?\d+.\d+?) \s+ (-?\d+.\d+?) \s+ (-?\d+.\d+?) \s* $",
						RegexOptions.IgnorePatternWhitespace | RegexOptions.IgnoreCase);

			if (match.Success)
			{
				result = _shapeConverter.ToEnvelope(new []
				{
					new CoordinateInfo
					{
						X = double.Parse(match.Groups[1].Value, CultureInfo.InvariantCulture),
						Y = double.Parse(match.Groups[2].Value, CultureInfo.InvariantCulture)
					},
					new CoordinateInfo
					{
						X = double.Parse(match.Groups[3].Value, CultureInfo.InvariantCulture),
						Y = double.Parse(match.Groups[4].Value, CultureInfo.InvariantCulture)
					},
				});
				return true;
			}
			result = null;
			return false;
		}
	}
}
