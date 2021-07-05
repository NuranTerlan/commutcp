using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;

namespace CustomHttpServer
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("Http-Listener service is started..\n");
            var ports = ConfigurationManager.AppSettings.Get("listening-ports")?
                .Trim().Split(';');
            // var uniquePortsForSure = new HashSet<string>(ports ?? new string[] {"2002"});
            
            using (var listener = new RequestListener(ports))
            {
                listener.Start();
            }

            Console.ReadLine();
        }
    }
}