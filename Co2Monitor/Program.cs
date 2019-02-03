using System;
using System.Threading.Tasks;

namespace Co2Monitor
{
	internal class Program
	{
		public static async Task Main()
		{
			var reader = new Co2Reader(dp => Console.WriteLine(dp));
			await reader.Read();
		}
	}
}
