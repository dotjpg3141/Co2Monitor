using HidLibrary;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace Co2Monitor
{
	public class Co2Reader
	{
		private readonly Action<DataPoint> OnDataPointReceived;

		public Co2Reader(Action<DataPoint> onDataPointReceived)
		{
			this.OnDataPointReceived = onDataPointReceived ?? throw new ArgumentNullException(nameof(this.OnDataPointReceived));
		}

		public async Task Read()
		{
			WriteLine("Waiting for device...", ConsoleColor.Gray);
			while (true)
			{
				var device = HidDevices.Enumerate(0x04D9, 0xA052).FirstOrDefault();
				if (device == null)
				{
					await Task.Delay(100);
					continue;
				}

				device.OpenDevice();
				WriteLine($"Device connected", ConsoleColor.Green);
				device.WriteFeatureData(new byte[9]); // encryption key

				while (device.IsConnected)
				{
					var result = await device.ReadReportAsync();
					var dp = Decode(result.Data);
					if (dp != null)
					{
						this.OnDataPointReceived(dp.Value);
					}
				}

				device.CloseDevice();
				WriteLine("Device disconnected.", ConsoleColor.Red);
				WriteLine("Waiting for device...", ConsoleColor.Gray);
			}
		}

		private static void WriteLine(object value, ConsoleColor color)
		{
			Console.ForegroundColor = color;
			Console.WriteLine(value);
			Console.ResetColor();
		}

		private static DataPoint? Decode(byte[] data)
		{
			if (data.Length != 8)
			{
				return null;
			}

			var value1 = (byte)(0xB9 + (data[4] >> 3 | data[2] << 5));
			var value2 = (byte)(0xAA + (data[0] >> 3 | data[4] << 5));
			var value = value1 << 8 | value2;

			switch ((byte)(data[2] >> 3 | data[3] << 5))
			{
				case 0xC6:
					return new DataPoint()
					{
						Type = DataPointType.Temperature,
						Value = value / 16.0 - 273.15,
					};

				case 0xD4:
					return new DataPoint()
					{
						Type = DataPointType.CO2,
						Value = value,
					};

				default:
					return null;
			}
		}
	}
}
