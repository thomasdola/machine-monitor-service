using System;
using System.Collections.Generic;
using MacMon.Models;
using Phoenix;

namespace MacMon.Commands
{
    public class Executor
    {
        private readonly Channel _channel;
        private readonly Profile _profile;

        private const string ChangePassword = "CHANGE_PASSWORD";
        private const string StartApplication = "START_APPLICATION";
        private const string StartService = "START_SERVICE";
        private const string StopApplication = "STOP_APPLICATION";
        private const string StopService = "STOP_SERVICE";
        private const string ShutDown = "SHUT_DOWN";

        private Executor(Channel channel, Profile profile)
        {
            _channel = channel;
            _profile = profile;
        }

        public static Executor Init(Channel channel, Profile profile)
        {
            Console.WriteLine("Init Command Listeners");
            return new Executor(channel, profile);
        }

        public void Start()
        {
            Console.WriteLine("Start Command Listeners");
            _channel.On(ChangePassword, message => Console.WriteLine("About to change password"));
            _channel.On(StartApplication, message => Console.WriteLine("About to Start process"));
            _channel.On(StopApplication, message => Console.WriteLine("About to Start process"));
            _channel.On(StartService, message => Console.WriteLine("About to Stop process"));
            _channel.On(StopService, message => Console.WriteLine("About to Stop process"));
            _channel.On(ShutDown, message => Console.WriteLine("About to Shut down machine"));
        }

        public void StartApplicationM(string path)
        {
            Application.Start(path);
        }

        public void StopApplicationM(string path)
        {
            Application.Stop(path);
        }

        public void StartServiceM(string name)
        {
            Service.Start(name);
        }

        public void StopServiceM(string name)
        {
            Service.Stop(name);
        }

        private void ShutDownM()
        {
            Power.Off();
        }
        
        private void RestartM()
        {
            Power.Restart();
        }

        private void ChangePasswordM(string password)
        {
            Account.Reset(_profile.Name, _profile.Username, password);
        }
    }
}