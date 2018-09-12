using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.NetworkInformation;
using System.Security.Principal;
using JsonFlatFileDataStore;
using MacMon.Commands;
using MacMon.Database;
using MacMon.Models;
using MacMon.Network;
using MacMon.Services.Http;
using MacMon.Services.WebSocket;
using NUnit.Framework;
using Phoenix;

namespace MacMon
{
    public class App
    {
        //need db
        private readonly DataStore _db;
        //need http client
        private readonly MacMonApi _api;
        //need websocket
        private Socket _socket;
        //need socket channel
        private Channel _channel;
        private Jobs.Executor _jobExecutor;

        public App()
        {
            Console.WriteLine("app init");
            _db = Store.InitStore();
            _api = new MacMonApi();
        }

        public void Start()
        {
            Console.WriteLine("app started");

            // Init dataStore with default data
            // Get Computer name, Log username and save them in db
            // Init Network listeners

            try
            {
                if (Connection.IsAvailable())
                {
                    DelayInit();
                    
                    Init();
                }
                else
                {
                    DelayInit();
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
        }
        
        public void Stop()
        {
            Console.WriteLine("app stopped");
            _jobExecutor.Stop();
            if (Connection.IsAvailable())
            {
                var machineIdentity = _db.GetItem<Identity>(Store.IdentityKey);
                _channel.Leave();
                _socket.Disconnect();
                _api.Logout(machineIdentity.Jwt);
            }
            _db.Dispose();
        }

        private void Init()
        {
            var userName = Env.GetComputerName();
            var password = Env.GetComputerName();
            
            var identity = _db.GetItem<Identity>(Store.IdentityKey);
            var job = _db.GetItem<Job>(Store.JobKey);
            
            if (identity.Uuid == null)
            {
                Console.WriteLine("Registering for new identity");
                identity = _api.Register(userName, password);
                _db.ReplaceItem(Store.IdentityKey, identity);
                job = UpdateCurrentJob(identity);
            }
            else
            {
                Console.WriteLine("App is logging in");
                identity = _api.GetIdentity(userName, password);
                _db.ReplaceItem(Store.IdentityKey, identity);
                job = UpdateCurrentJob(identity);
                
            }
            
            _socket = MacMonWebSocket.InitSocket(identity.Jwt);
            InitSocketMonitor(_socket);
            
            _channel = _socket.MakeChannel($"MACHINE:{identity.Uuid}");
            JoinChannel(_channel, identity);

            Executor.Init(_channel).Start();

            _jobExecutor = Jobs.Executor.Init(_channel, _db);
            _jobExecutor.Start(job);
        }

        private static void DelayInit()
        {
            var netHandlers = new List<NetworkAvailabilityChangedEventHandler> {WhenNetworkAvailabilityChanged};
            
            Monitor.Init(netHandlers);
            
            Console.WriteLine("System in Standby mode -> waiting for network availability");
        }
        
        private static void WhenNetworkAvailabilityChanged(object sender, NetworkAvailabilityEventArgs e)
        {
            if (e.IsAvailable)
            {
                Console.WriteLine("Network Available");
                new App().Start();
            }
            else
            {
                Console.WriteLine("Network Unavailable");
            }
        }
        
        private static void JoinChannel(Channel channel, Identity identity)
        {
            InitChannelMonitor(channel);
            
            var auth = new Dictionary<string, object>
            {
                {"token", identity.Jwt.Token}
            };
            
            Console.WriteLine("Channel ------> {0}", channel.topic);

            channel.Join(auth)
                .Receive(Reply.Status.Ok, r => Console.WriteLine("Joined channel successfully -> {0}", r))
                .Receive(Reply.Status.Error, r => Console.WriteLine("Joined channel failed -> {0}", r));
        }
        
        
        private static void InitChannelMonitor(Channel channel)
        {
            channel.On(Message.InBoundEvent.phx_close, m => { Console.WriteLine("Channel closed -> {0}", m); });
            channel.On(Message.InBoundEvent.phx_error, m => { Console.WriteLine("Channel error -> {0}", m); });
            channel.On("after_join", m => { Console.WriteLine("Joined Channel -> {0}", m); });
        }

        private static void InitSocketMonitor(Socket socket)
        {
            socket.OnOpen = () => { Console.WriteLine("socket connected.."); };

            socket.OnClose = (code, message) => { Console.WriteLine("socket Closed.."); };

            socket.OnError = message => { Console.WriteLine("socket connection error.."); };
        }

        private Job UpdateCurrentJob(Identity machineIdentity)
        {
            var currentJob = _api.GetJob(machineIdentity.Jwt);
            if (currentJob != null)
            {
                _db.ReplaceItem(Store.JobKey, currentJob);
            }

            return currentJob;
        }
    }

    public static class Env
    {
        public static string GetComputerName()
        {
            return WindowsIdentity.GetCurrent().Name.Split('\\').ToArray().First();
        }

        public static string GetUsername()
        {
            return WindowsIdentity.GetCurrent().Name.Split('\\').ToArray().Last();
        }
    }
}