using System;

namespace Co2Monitor
{
	public class DataManager
	{
		public double LastTemperature { get; set; }
		public double LastCo2 { get; set; }
		public DateTime LastTime { get; set; }

		public void Add(DataPoint point)
		{
			this.LastTime = point.Time;
			switch (point.Type)
			{
				case DataPointType.Temperature:
					this.LastTemperature = point.Value;
					break;

				case DataPointType.CO2:
					this.LastCo2 = point.Value;
					break;

				default:
					throw new InvalidOperationException();
			}
		}
	}
}
