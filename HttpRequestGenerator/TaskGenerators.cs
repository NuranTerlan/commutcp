using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace HttpRequestGenerator
{
    public static class TaskGenerators
    {
        public static IEnumerable<Task> GenerateNHttpClient(int n, int reqsPerClient)
        {
            var returnedTasks = new List<Task>();
            var clientHelper = new ClientHelper(n);
            
            for (var i = 0; i < n; i++)
            {
                returnedTasks.Add(Task.Run(() => clientHelper
                    .SendNPostRequestsParallel(reqsPerClient, "Message from Client# " + Guid.NewGuid())));
            }

            return returnedTasks;
        }
    }
}