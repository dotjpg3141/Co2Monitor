using System;

namespace Co2Monitor
{
	public struct DataPoint
	{
		public DataPointType Type { get; set; }
		public double Value { get; set; }

		public override string ToString()
		{
			switch (this.Type)
			{
				case DataPointType.Temperature:
					return $"Temp: {this.Value:0.0} °C";

				case DataPointType.CO2:
					return $"CO₂:  {this.Value} ppm";

				default:
					throw new InvalidOperationException();
			}
		}
	}
}
