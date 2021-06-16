using System;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace CustomHttpServer
{
    public class Server : IDisposable
    {
        private bool _isRunning;
        private readonly TcpListener _listener;

        public Server(short port)
        {
            AppContext.SetSwitch("System.Net.Http.SocketsHttpHandler.Http2UnencryptedSupport", true);
            _listener = new TcpListener(IPAddress.Parse("127.0.1.1"), port);
        }

        public async Task StartAsync()
        {
            Console.WriteLine("Http server is started!");
            await RunServer();
        }

        private async Task RunServer()
        {
            _isRunning = true;
            _listener.Start();

            while (_isRunning)
            {
                Console.WriteLine("\nWaiting for a client to establish a connection..");
                using (var client = await _listener.AcceptTcpClientAsync())
                {
                    var clientId = "client/" + Guid.NewGuid();
                    Console.WriteLine($"New client came ({clientId}), connection is established!");
                    try
                    {
                        await HandleRequestingClient(client, clientId);
                        Console.WriteLine($"Request is received from {clientId}, and handled successfully!");
                    }
                    catch (Exception e)
                    {
                        Console.WriteLine(
                            $"Something went wrong while handling the request that coming from {clientId}" +
                            $"\nError content: {e.Message}");
                    }
                }
            }
        }

        public void Stop()
        {
            _isRunning = false;
            _listener.Stop();
        }

        private async Task HandleRequestingClient(TcpClient client, string clientId)
        {
            await using (var stream = client.GetStream())
            {
                var data = new byte[256];
                var bytes = await stream.ReadAsync(data, 0, data.Length);
                var resMessage = Encoding.UTF8.GetString(data, 0, bytes);
                Console.WriteLine(resMessage);
            }
        }

        public void Dispose()
        {
            Stop();
        }
    }
}