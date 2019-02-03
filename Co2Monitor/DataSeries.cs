using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;

namespace Co2Monitor
{
	public class DataSeries
	{
		internal static readonly CultureInfo Format = CultureInfo.InvariantCulture;

		private readonly string filePath;
		private readonly object lockObject = new object();
		private readonly List<(DateTime, double)> list = new List<(DateTime, double)>();

		internal DataSeries(string filePath, IEnumerable<(DateTime, double)> collection)
		{
			this.filePath = filePath;
			this.list = new List<(DateTime, double)>(collection);
			this.LastValue = this.list.LastOrDefault().Item2;
		}

		public double LastValue { get; internal set; }

		public void Add(DateTime time, double value)
		{
			this.LastValue = value;

			lock (this.lockObject)
			{
				this.list.Add((time, value));
				using (var writer = new StreamWriter(this.filePath, true, Encoding.UTF8))
				{
					writer.WriteLine(time.ToString("o", Format) + "\t" + value.ToString(Format));
				}
			}
		}

		public (DateTime, double)[] GetData()
		{
			lock (this.lockObject)
			{
				return this.list.ToArray();
			}
		}
	}
}
