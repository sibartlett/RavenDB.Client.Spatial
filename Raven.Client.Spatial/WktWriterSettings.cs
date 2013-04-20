namespace Raven.Client.Spatial
{
	public class WktWriterSettings
	{
		public WktWriterSettings()
		{
			//LinearRing = false;
			//Triangle = false;
			DimensionFlag = false;
			MaxDimesions = 4;
		}

		public int MaxDimesions { get; set; }
		public bool DimensionFlag { get; set; }
		//public bool LinearRing { get; set; }
		//public bool Triangle { get; set; }

		public static WktWriterSettings Raven20
		{
			get { return new WktWriterSettings(); }
		}

		public static WktWriterSettings Raven25
		{
			get
			{
				return new WktWriterSettings
				{
					DimensionFlag = false,
					MaxDimesions = 2
				};
			}
		}
	}
}
