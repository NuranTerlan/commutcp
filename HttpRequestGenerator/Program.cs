using System;
using System.Configuration;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;

namespace HttpRequestGenerator
{
    class Program
    {
        static async Task Main(string[] args)
        {
            var configurationManager = ConfigurationManager.AppSettings;

            if (!int.TryParse(configurationManager.Get("clients"), out var clients))
            {
                Console.WriteLine("Set clients count correctly (!number only!) in App.config file!!");
                return;
            }

            if (!int.TryParse(configurationManager.Get("requests-per-client"), out var reqsPerClient))
            {
                Console.WriteLine("Set requests count correctly (!number only!) in App.config file!!");
                return;
            }
            
            Console.WriteLine("Clients and requests are generated for execution..\n");
            await Task.Delay(TimeSpan.FromSeconds(2));
            var watchFroParallel = Stopwatch.StartNew();
            var tasks = TaskGenerators.GenerateNHttpClient(clients, reqsPerClient);
            Task.WaitAll(tasks.ToArray());
            watchFroParallel.Stop();
            Console.WriteLine($"All done. {reqsPerClient * clients} requests are sent successfully!\n" +
                              "\tTotal time taken for execution: " + watchFroParallel.Elapsed + " (TimeSpan)");

            Console.WriteLine(
                "Press any key to exit. But be careful, don't exit before all requests handled that are pipelined in CustomHttpServer Windows Service");
            Console.ReadLine();
        }
    }
}