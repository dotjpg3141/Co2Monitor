using System;

namespace Co2Monitor
{
	public class Logger
	{
		public Logger(string tag)
		{
			this.Tag = tag;
		}

		public string Tag { get; }

		public void Verbose(object value)
			=> WriteLine("VERBOSE", value, ConsoleColor.Gray);

		public void Info(object value)
			=> WriteLine("INFO", value, ConsoleColor.White);

		public void Error(object value)
			=> WriteLine("ERROR", value, ConsoleColor.Red);

		private void WriteLine(string level, object value, ConsoleColor color)
		{
			Console.ForegroundColor = color;
			Console.WriteLine($"[{this.Tag}] - [{level}]: {value}");
			Console.ResetColor();
		}
	}
}
