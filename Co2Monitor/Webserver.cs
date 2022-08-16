using System;
using System.Net;
using System.Threading.Tasks;

namespace Co2Monitor
{
    public class Webserver
    {
        private readonly HttpListener listener = new();

        public Logger Logger { get; set; } = new Logger("Webserver");
        public int Port { get; }

        public Webserver(int port)
        {
            if (!HttpListener.IsSupported)
            {
                throw new PlatformNotSupportedException("HttpListener is not supportet.");
            }
            this.Port = port;
            this.listener.Prefixes.Add($"http://+:{port}/");
        }


        public async Task Run(Func<HttpListenerContext, Task> handleConnection)
        {
            try
            {
                this.listener.Start();
            }
            catch (HttpListenerException ex)
            {
                throw new Exception(@"
Troubleshooting:
https://serverfault.com/a/571817
https://stackoverflow.com/a/18856394
https://stackoverflow.com/a/17824452
".Trim(), ex);
            }

            this.Logger.Verbose($"Webserver running at port {this.Port} ...");

            while (this.listener.IsListening)
            {
                var context = await this.listener.GetContextAsync();
                try
                {
                    await handleConnection(context);
                }
                catch (Exception ex)
                {
                    this.Logger.Error(ex);
                }
            }
        }

        public void Stop()
        {
            this.listener.Stop();
            this.listener.Close();
        }
    }
}
