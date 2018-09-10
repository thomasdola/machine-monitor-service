using System;
using System.Collections.Generic;
using Castle.Core.Internal;
using JsonFlatFileDataStore;
using MacMon.Models;
using Phoenix;

namespace MacMon.Jobs
{
    public class Executor
    {
        private readonly Channel _channel;
        private readonly DataStore _store;
        private readonly Identity _identity;

        private Executor(Channel channel, DataStore store, Identity identity)
        {
            _channel = channel;
            _store = store;
            _identity = identity;
        }

        public static Executor Init(Channel channel, DataStore store, Identity identity)
        {
            //socket = JoinSocketChannel(socket, identity);
            return new Executor(channel, store, identity);
        }

        public void Start(Job job)
        {
            Console.WriteLine("Jobs Running with -> {0} - {1}", _identity.Uuid, _identity.Jwt.Token);
            Console.WriteLine("Jobs Running..., {0} - {1}", job.Network, job.UserActivity);
            
            if (job.Network)
            {
                var data = new Dictionary<string, object>{{ "IP", "12.12.12.35"}};
                _channel.Push(Services.WebSocket.MacMonWebSocket.NETWORK_STATUS_CHANGED, data);
                Console.WriteLine("Network Job Running...");   
            }

            if (job.UserActivity)
            {
                Console.WriteLine("UserActivityLog Job Running...");
            }

            if (!job.Applications.IsNullOrEmpty())
            {
                foreach (var application in job.Applications)
                {
                    Console.WriteLine("Application {0} Job Running...", application);
                }   
            }

            if (!job.Services.IsNullOrEmpty())
            {
                foreach (var service in job.Services)
                {
                    Console.WriteLine("Service {0} Job Running...", service);
                }
            }
        }
    }
}