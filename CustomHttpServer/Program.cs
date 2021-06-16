using System;
using System.Configuration;

namespace CustomHttpServer
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("Http-Listener service is started..\n");
            var ports = ConfigurationManager.AppSettings.Get("listening-ports")?
                .Trim().Split(';');
            using (var listener = new RequestListener(ports))
            {
                listener.Start();
            }

            Console.ReadLine();
        }
    }
}