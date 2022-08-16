using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading.Tasks;

namespace Co2Monitor
{
	internal class Co2Webserver
	{
		public async Task Run(DataManager manager, int port)
		{
			var server = new Webserver(port);

			if (Debugger.IsAttached)
			{
				OpenUrl("http://localhost:" + port);
			}

			var indexPath = "data/index.html";
			var indexContent = File.ReadAllText(indexPath);

			await server.Run((context) =>
			{
				server.Logger.Info($"{context.Request.HttpMethod} {context.Request.Url} from {context.Request.RemoteEndPoint}");

				if (Debugger.IsAttached)
				{
					var path = Path.GetFullPath(Path.Combine("../../..", indexPath));
					indexContent = File.ReadAllText(path);
				}

				var time = manager.LastTime;
				var temperature = manager.Temperature.LastValue;
				var co2 = manager.Co2.LastValue;

                using var output = new StreamWriter(context.Response.OutputStream);
                switch (context.Request.RawUrl?.ToLowerInvariant().TrimEnd('/'))
                {
                    case "/update":
                        context.Response.AddHeader("Content-Type", "application/json; charset=utf-8");
                        output.WriteLine(ToJson(new
                        {
                            Time = time,
                            Temperature = temperature,
                            Co2 = co2,
                        }));
                        break;

                    default:
                        output.WriteLine(indexContent
                            .Replace("$time$", ToJson(time))
                            .Replace("$temperature$", ToJson(temperature))
                            .Replace("$co2$", ToJson(co2))
                            .Replace("$temperatureHistory$", ToJson(manager.Temperature))
                            .Replace("$co2History$", ToJson(manager.Co2)));
                        break;
                }
            });
		}

		private static string ToJson(object value)
		{
			return JsonConvert.SerializeObject(value, new JsonSerializerSettings()
			{
				ContractResolver = new CamelCasePropertyNamesContractResolver(),
			});
		}
		private static string ToJson(DataSeries series)
		{
			var minDate = DateTime.Today.AddDays(-7);
			return ToJson(series.GetData().Where(item => item.Item1 >= minDate).Select(item => new
			{
				X = item.Item1,
				Y = item.Item2,
			}));
		}

		private static void OpenUrl(string url)
		{
			try
			{
				Process.Start(url);
			}
			catch
			{
				// hack because of this: https://github.com/dotnet/corefx/issues/10361
				if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
				{
					url = url.Replace("&", "^&");
					Process.Start(new ProcessStartInfo(url) { UseShellExecute = true });
				}
				else if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
				{
					Process.Start("xdg-open", url);
				}
				else if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
				{
					Process.Start("open", url);
				}
				else
				{
					throw;
				}
			}
		}
	}
}
