using System;
using System.Collections.Specialized;
using System.Configuration;
using System.Diagnostics.CodeAnalysis;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace HttpRequestGenerator
{
    public class ClientHelper
    {
        private readonly HttpClient _client;
        private static readonly string Port = ConfigurationManager.AppSettings.Get("port");

        private static readonly int MaxTcpConnections =
            int.Parse(ConfigurationManager.AppSettings.Get("max-tcp-connections") ?? "1");

        public ClientHelper(int maxTcpConnections)
        {
            _client = new HttpClient(new HttpClientHandler {MaxConnectionsPerServer = maxTcpConnections});

            // Client.Timeout = Timeout.InfiniteTimeSpan;
            // var servicePointMgr = ServicePointManager.FindServicePoint(uri);
            // servicePointMgr.ConnectionLimit = 50;
            // ServicePointManager.DefaultConnectionLimit = 50;
        }

        // public static async Task SendNPostRequestsAsync(int count, short port, string message)
        // {
        //     ServicePointManager.UseNagleAlgorithm = false;
        //     ServicePointManager.Expect100Continue = false;
        //     ServicePointManager.DefaultConnectionLimit = int.MaxValue;
        //     ServicePointManager.EnableDnsRoundRobin = true;
        //     ServicePointManager.ReusePort = true;
        //     ServicePointManager.SecurityProtocol =
        //         SecurityProtocolType.Tls | SecurityProtocolType.Tls11 | SecurityProtocolType.Tls12;
        //
        //     var token = new CancellationToken();
        //     for (int i = 0; i < count; i++)
        //     {
        //         using (var request = new HttpRequestMessage(HttpMethod.Post, "http://localhost:" + port + '/'))
        //         {
        //             using (var stringContent = new StringContent(message, Encoding.UTF8, "text/plain"))
        //             {
        //                 request.Content = stringContent;
        //
        //                 using (var response =
        //                     await Client.SendAsync(request, HttpCompletionOption.ResponseHeadersRead, token))
        //                 {
        //                     Console.WriteLine("sent: " + (i + 1));
        //                     response.EnsureSuccessStatusCode();
        //                 }
        //             }
        //         }
        //     }
        // }

        public void SendNPostRequestsParallel(int count, string message)
        {
            Parallel.For(0, count, async (i) =>
            {
                try
                {
                    var content = new StringContent(message, Encoding.UTF8, "text/plain");
                    var response = await _client.PostAsync("http://localhost:" + Port + '/', content);
                    response.EnsureSuccessStatusCode();
                }
                catch (Exception e)
                {
                    Console.WriteLine("Can't send request! " + e.Message);
                    throw;
                }
            });
        }
    }
}