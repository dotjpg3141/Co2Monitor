﻿using System;
using System.Diagnostics;
using System.Threading.Tasks;

namespace Co2Monitor
{
    internal class Program
    {
        private readonly DataManager manager = new();

        public static async Task Main()
        {
            try
            {
                var program = new Program();
                await program.Run();
            }
            catch (AggregateException ex)
            {
                foreach (var inner in ex.InnerExceptions)
                {
                    Console.WriteLine(inner.Message);
                }
                Console.WriteLine();
                Console.WriteLine(ex);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
            }
        }

        public async Task Run()
        {
            var task = await Task.WhenAny(HandleCo2Reader(), HandleWebserver());
            task.Wait();
        }

        private async Task HandleCo2Reader()
        {
            var reader = new Co2Reader();
            await reader.Read(dp =>
            {
                reader.Logger.Info(dp);
                this.manager.Add(dp);
            });
        }

        private async Task HandleWebserver()
        {
            var webserver = new Co2Webserver();
            await webserver.Run(this.manager, 8080);
        }
    }
}
