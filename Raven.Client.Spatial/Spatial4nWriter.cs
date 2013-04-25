using System.Globalization;

namespace Raven.Client.Spatial
{
	internal class Spatial4nWriter
	{
		private readonly IShapeConverter _shapeConverter;
		private readonly WktWriter _wktWriter;

		public Spatial4nWriter(IShapeConverter shapeConverter)
		{
			_shapeConverter = shapeConverter;
			_wktWriter = new WktWriter(shapeConverter);
		}

		public string Write(object geometry)
		{
			if (ReferenceEquals(null, geometry))
				return null;

			string result;

			if (TryWriteCircle(geometry, out result))
				return result;

			if (TryWriteEnvelope(geometry, out result))
				return result;

			return _wktWriter.Write(geometry);
		}

		private bool TryWriteCircle(object shape, out string result)
		{
			if (_shapeConverter.CanConvert(ObjectType.Circle) &&
				_shapeConverter.GetObjectType(shape) == ObjectType.Circle)
			{
				var circle = _shapeConverter.FromCircle(shape);
				result = string.Format(CultureInfo.InvariantCulture, "CIRCLE({0:F9} {1:F9} d={2:F9})", circle[0], circle[1], circle[2]);
				return true;  
			}
			result = default(string);
			return false;
		}

		private bool TryWriteEnvelope(object shape, out string result)
		{
			var a = _shapeConverter.CanConvert(ObjectType.Envelope);
			var b = _shapeConverter.GetObjectType(shape) == ObjectType.Envelope;
			if (_shapeConverter.CanConvert(ObjectType.Envelope) &&
				_shapeConverter.GetObjectType(shape) == ObjectType.Envelope)
			{
				var envelope = _shapeConverter.FromEnvelope(shape);
				result = string.Format(CultureInfo.InvariantCulture, "{0:F9} {1:F9} {2:F9} {3:F9}", envelope[0].X, envelope[0].Y, envelope[1].X, envelope[1].Y);
				return true;
			}
			result = default(string);
			return false;
		}
	}
}
