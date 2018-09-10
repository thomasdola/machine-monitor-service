using System;
using System.Collections.Generic;
using JsonFlatFileDataStore;
using MacMon.Database;
using MacMon.Models;
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

        public App()
        {
            Console.WriteLine("app init");
            _db = Store.InitStore();
            _api = new MacMonApi();
        }

        public void Start()
        {
            Console.WriteLine("app started");
            
            
            Identity machineIdentity = _db.GetItem<Identity>(Store.IdentityKey);
            Job currentJob = _db.GetItem<Job>(Store.JobKey);
            
            Console.WriteLine("app started -> Identity {0}", machineIdentity.Uuid == null);
            Console.WriteLine("app started -> Current Job {0}", currentJob == null);

            Profile profile = _db.GetItem<Profile>(Store.ProfileKey);

            if (machineIdentity.Uuid == null)
            {
                Console.WriteLine("Registering for new identity");
                // Machine needs to register for a new IDENTITY
                try
                {
                    machineIdentity = _api.Register(profile.Name, profile.Password);
                    _db.ReplaceItem(Store.IdentityKey, machineIdentity);
                    currentJob = UpdateCurrentJob(machineIdentity);
                }
                catch (Exception e)
                {
                    Console.WriteLine(e.Message);
                }
            }
            else
            {
                Console.WriteLine("App is logging in");
                try
                {
                    machineIdentity = _api.GetIdentity(profile.Name, profile.Password);
                    _db.ReplaceItem(Store.IdentityKey, machineIdentity);
                    currentJob = UpdateCurrentJob(machineIdentity);
                }
                catch(Exception e)
                {
                    Console.WriteLine(e.Message);
                }
                
            }

            _socket = MacMonWebSocket.InitSocket(machineIdentity.Jwt);
            InitSocketMonitor(_socket);

            
            _channel = _socket.MakeChannel($"MACHINE:{machineIdentity.Uuid}");
            var channelParam = new Dictionary<string, object> {
                { "token", machineIdentity.Jwt.Token }
            };
            InitChannelMonitor(_channel);

            _channel.Join(channelParam)
                .Receive(Reply.Status.Ok, r => Console.WriteLine("Joined channel successfully -> {0}", r))
                .Receive(Reply.Status.Error, r => Console.WriteLine("Joined channel failed -> {0}", r));


            Commands.Executor.Init(_channel, profile).Start();

            Jobs.Executor.Init(_channel, _db, machineIdentity).Start(currentJob);
        }

        public void Stop()
        {
            Console.WriteLine("app stopped");
            Identity machineIdentity = _db.GetItem<Identity>(Store.IdentityKey);
            _channel.Leave();
            _socket.Disconnect();
            _api.Logout(machineIdentity.Jwt);
            _db.Dispose();
        }
        
        
        private void InitChannelMonitor(Channel channel)
        {
            channel.On(Message.InBoundEvent.phx_close, m => { Console.WriteLine("Channel closed -> {0}", m); });
            channel.On(Message.InBoundEvent.phx_error, m => { Console.WriteLine("Channel error -> {0}", m); });
            channel.On("after_join", m => { Console.WriteLine("Joined Channel -> {0}", m); });
        }

        private void InitSocketMonitor(Socket socket)
        {
            socket.OnOpen = () => { Console.WriteLine("socket connected.."); };

            socket.OnClose = (code, message) => { Console.WriteLine("socket Closed.."); };

            socket.OnError = message => { Console.WriteLine("socket connection error.."); };
        }

        private Job UpdateCurrentJob(Identity machineIdentity)
        {
            Job currentJob;
            currentJob = _api.GetJob(machineIdentity.Jwt);
            if (currentJob != null)
            {
                _db.ReplaceItem(Store.JobKey, currentJob);
            }

            return currentJob;
        }
    }

    [TestFixture]
    public class AppTest
    {
        [Test]
        public void StartTest()
        {
            Assert.That(true, Is.EqualTo(true));
        }
    }
}