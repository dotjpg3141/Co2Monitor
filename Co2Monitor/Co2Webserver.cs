using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Threading.Tasks;

namespace Co2Monitor
{
	internal class Co2Webserver
	{
		private const string TimeKey = "time";
		private const string TemperatureKey = "temperature";
		private const string CO2Key = "co2";

		public async Task Run(DataManager manager, int port)
		{
			var server = new Webserver(8080);

			if (Debugger.IsAttached)
			{
				Process.Start("http://localhost:" + port);
			}

			var indexPath = "data/index.html";
			var indexContent = File.ReadAllText(indexPath);

			await server.Run((context) =>
			{
				server.Logger.Info($"{context.Request.HttpMethod} {context.Request.Url} from {context.Request.RemoteEndPoint}");

				if (Debugger.IsAttached)
				{
					indexContent = File.ReadAllText(Path.Combine("../..", indexPath));
				}

				var time = manager.LastTime.ToString("o");
				var temperature = manager.LastTemperature.ToString(CultureInfo.InvariantCulture);
				var co2 = manager.LastCo2.ToString(CultureInfo.InvariantCulture);

				using (var output = new StreamWriter(context.Response.OutputStream))
				{
					switch (context.Request.RawUrl.ToLowerInvariant().TrimEnd('/'))
					{
						case "/update":
							context.Response.AddHeader("Content-Type", "application/json; charset=utf-8");
							WriteJson(output, time, temperature, co2);
							break;

						default:
							output.WriteLine(indexContent
								.Replace($"${TimeKey}$", time)
								.Replace($"${TemperatureKey}$", temperature)
								.Replace($"${CO2Key}$", co2));
							break;
					}
				}
			});
		}

		private static void WriteJson(StreamWriter output, string time, string temperature, string co2)
		{
			output.WriteLine($@"
{{ 
	""{TimeKey}"":        ""{time}"", 
	""{TemperatureKey}"": {temperature}, 
	""{CO2Key}"":         {co2}
}}");
		}
	}
}
