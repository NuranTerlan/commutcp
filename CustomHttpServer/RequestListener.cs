using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace CustomHttpServer
{
    public class RequestListener : IDisposable
    {
        private readonly HttpListener _listener;
        private readonly CancellationToken _listeningToken;

        private readonly SemaphoreSlim _semaphore;

        private readonly List<string> _messagesToWriteConsole;
        private int _handledRequests;
        
        // private readonly Queue<HttpListenerRequest> _requestsToHandle;

        
        public RequestListener(params string[] ports)
        {
            if (!HttpListener.IsSupported)
            {
                Console.WriteLine("Windows XP SP2 or Server 2003 is required to use the HttpListener class.");
                return;
            }

            if (ports is null || ports.Length == 0)
            {
                throw new ArgumentException("At least one port required to start listening!");
            }

            _messagesToWriteConsole = new List<string>();
            _handledRequests = 0;
            _semaphore = new SemaphoreSlim(1, 1);
            _listener = new HttpListener();
            _listener.TimeoutManager.IdleConnection = TimeSpan.FromMinutes(10);
            _listeningToken = new CancellationToken();

            foreach (var port in ports)
            {
                _listener.Prefixes.Add("http://localhost:" + port + '/');
            }
        }

        public void Start()
        {
            try
            {
                _listener.Start();
                Console.WriteLine($"Listening started for {string.Join(", ", _listener.Prefixes)}\n");

                // OLD REQUEST CATCHING LOGIC
                // while (true)
                // {
                //     // await _semaphore.WaitAsync(_listeningToken);
                //     // var context = await _listener.GetContextAsync();
                //     // var request = context.Request;
                //     
                //     // using (var reader = new StreamReader(request.InputStream, request.ContentEncoding))
                //     // {
                //     //     Console.WriteLine(await reader.ReadToEndAsync());
                //     //     // _semaphore.Release();
                //     // }
                //     // await Task.Delay(TimeSpan.FromMilliseconds(1000), _listeningToken);
                //     
                //     // context.Response.Close();
                // }

                /* NEW LOGIC */
                _listener.BeginGetContext(async ar => await OnContextReceiving(ar), _listener);
                Task.Run(async () => await WriteToConsole(), _listeningToken);
                // HandleRequests();
            }
            catch (Exception e)
            {
                Console.WriteLine("Something went wrong while listening process!\nError content: " + e.Message);
                throw;
            }

            Console.ReadLine();
        }

        private async Task OnContextReceiving(IAsyncResult asyncRes)
        {
            try
            {
                var context = _listener.EndGetContext(asyncRes);
                var request = context.Request;
                var response = context.Response;
                
                // response.AddHeader("Access-Control-Allow-Headers", "Content-Type, Accept, X-Requested-With");
                // response.AddHeader("Content-Type", "text/plain");
                // response.AddHeader("Access-Control-Allow-Methods", "GET, POST");
                // response.AddHeader("Access-Control-Max-Age", "1728000");
                // response.AppendHeader("Access-Control-Allow-Origin", "*");

                // await _semaphore.WaitAsync(_listeningToken);
                // context.Response.Close();
                // _requestsToHandle.Enqueue(context.Request);
                // _semaphore.Release();

                _listener.BeginGetContext(async ar => await OnContextReceiving(ar), _listener);

                using (var reader = new StreamReader(request.InputStream, request.ContentEncoding))
                {
                    await _semaphore.WaitAsync(_listeningToken);
                    _messagesToWriteConsole.Add(await reader.ReadToEndAsync());
                    _semaphore.Release();
                }

                var buffer = Encoding.UTF8.GetBytes("Handled!");
                response.ContentLength64 = buffer.Length;
                using (var output = response.OutputStream)
                {
                    await output.WriteAsync(buffer.AsMemory(0, buffer.Length), _listeningToken);
                }

                response.Close();

                // simulate 3 seconds handling work per request
                await Task.Delay(TimeSpan.FromSeconds(5), _listeningToken);
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
                // Console.WriteLine($"Listener could handle {_handledRequests} requests..");
            }
        }

        private async Task WriteToConsole()
        {
            var tryCount = 0;
            while (true)
            {
                if (++tryCount < 2)
                {
                    Console.WriteLine($"WriteToConsole checks for new messages.. ({_handledRequests} requests are handled)");
                }
                await _semaphore.WaitAsync(_listeningToken);
                if (_messagesToWriteConsole.Any())
                {
                    tryCount = 0;
                    Console.WriteLine($"{_messagesToWriteConsole.Count} messages received!");
                    // Console.WriteLine(string.Join('\n', _messagesToWriteConsole));
                    _handledRequests += _messagesToWriteConsole.Count;
                    _messagesToWriteConsole.Clear();
                }
                _semaphore.Release();
                await Task.Delay(TimeSpan.FromSeconds(1), _listeningToken);
            }
        }

        // private IEnumerable<HttpListenerRequest> GetRequests()
        // {
        //     while (true)
        //     {
        //         _semaphore.Wait(_listeningToken);
        //         _requestsToHandle.TryDequeue(out var returnedRequest);
        //         _semaphore.Release();
        //
        //         yield return returnedRequest;
        //
        //         /*var context = _listener.GetContext();
        //         yield return Tuple.Create(context.Request, context.Response);*/
        //     }
        // }

        // public void HandleRequests()
        // {
        //     Parallel.ForEach(GetRequests(), async (request) =>
        //     {
        //         await _semaphore.WaitAsync(_listeningToken);
        //         Console.WriteLine(++_handledRequests);
        //         _semaphore.Release();
        //
        //         // var (request, response) = combination;
        //         using (var reader = new StreamReader(request.InputStream, request.ContentEncoding))
        //         {
        //             Console.WriteLine(await reader.ReadToEndAsync());
        //         }
        //
        //         // response.Close();
        //
        //         // simulate 3 seconds handling work per request
        //         await Task.Delay(TimeSpan.FromSeconds(3), _listeningToken);
        //     });
        // }

        public void Stop()
        {
            _listener.Stop();
        }

        public void Dispose()
        {
            Stop();
            _listener.Close();
            Console.WriteLine($"Listener could handle {_handledRequests} requests..");
        }
    }
}