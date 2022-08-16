using HidLibrary;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace Co2Monitor
{
	public class Co2Reader
	{
		public bool CurrentlyConnected { get; private set; }
		public Logger Logger { get; set; } = new Logger("CO₂-Monitor");

		public async Task Read(Action<DataPoint> onDataPointReceived)
		{
			this.Logger.Verbose("Waiting for device...");
			while (true)
			{
				var device = HidDevices.Enumerate(0x04D9, 0xA052).FirstOrDefault();
				if (device == null)
				{
					await Task.Delay(100);
					continue;
				}

				device.OpenDevice();
				this.Logger.Verbose($"Device connected");
				device.WriteFeatureData(new byte[9]); // encryption key
				this.CurrentlyConnected = true;

				await Task.Delay(10000); // wait that device is initialized

				while (device.IsConnected)
				{
					var result = device.ReadReport();
					var dp = Decode(result.Data);
					if (dp != null)
					{
						onDataPointReceived(dp.Value);
					}
				}

				device.CloseDevice();
				this.CurrentlyConnected = false;
				this.Logger.Error("Device disconnected.");
				this.Logger.Verbose("Waiting for device...");
			}
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
						Time = DateTime.UtcNow,
					};

				case 0xD4:
					return new DataPoint()
					{
						Type = DataPointType.CO2,
						Value = value,
						Time = DateTime.UtcNow,
					};

				default:
					return null;
			}
		}
	}
}
