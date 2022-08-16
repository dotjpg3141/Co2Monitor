using System;
using System.IO;
using System.Linq;

namespace Co2Monitor
{
	public class DataManager
	{
		public Logger Logger { get; set; } = new Logger("DataManager");
		public DataSeries Temperature { get; }
		public DataSeries Co2 { get; }
		public DateTime LastTime { get; private set; }

		public event Action? DataPointAdded;

		public DataManager()
		{
			this.Temperature = LoadSeriesFromFile("data/temperature.csv");
			this.Co2 = LoadSeriesFromFile("data/co2.csv");
		}

		public void Add(DataPoint point)
		{
			try
			{
				switch (point.Type)
				{
					case DataPointType.Temperature:
						this.Temperature.Add(point.Time, point.Value);
						break;

					case DataPointType.CO2:
						this.Co2.Add(point.Time, point.Value);
						break;

					default:
						throw new InvalidOperationException();
				}
			}
			catch (IOException ex)
			{
				this.Logger.Error("Could not write series data: " + ex);
			}

			this.LastTime = point.Time;
			DataPointAdded?.Invoke();
		}

		private DataSeries LoadSeriesFromFile(string path)
		{
			if (File.Exists(path))
			{
				this.Logger.Info($"Try reading file: {path}");
				try
				{
					var items = File.ReadAllLines(path)
					.Select(item =>
					{
						var data = item.Split('\t');
						var time = DateTime.Parse(data[0], DataSeries.Format);
						var value = double.Parse(data[1], DataSeries.Format);
						return (time, value);
					});

					return new DataSeries(path, items);
				}
				catch (Exception ex)
				{
					this.Logger.Error(ex);
				}
			}

			return new DataSeries(path, Enumerable.Empty<(DateTime, double)>());
		}
	}
}
