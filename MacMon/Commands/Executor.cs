using System;
using System.Collections.Generic;
using MacMon.Models;
using Phoenix;

namespace MacMon.Commands
{
    public class Executor
    {
        private readonly Channel _channel;

        private const string ChangePassword = "CHANGE_PASSWORD";
        private const string StartApplication = "START_APPLICATION";
        private const string StartService = "START_SERVICE";
        private const string StopApplication = "STOP_APPLICATION";
        private const string StopService = "STOP_SERVICE";
        private const string ShutDown = "SHUT_DOWN";

        private Executor(Channel channel)
        {
            _channel = channel;
        }

        public static Executor Init(Channel channel)
        {
            Console.WriteLine("Init Command Listeners");
            return new Executor(channel);
        }

        public void Start()
        {
            Console.WriteLine("Start Command Listeners");
            _channel.On(ChangePassword, message => Console.WriteLine("About to change password -> {0}", message));
            _channel.On(StartApplication, message => Console.WriteLine("About to Start process -> {0}", message));
            _channel.On(StopApplication, message => Console.WriteLine("About to Start process -> {0}", message));
            _channel.On(StartService, message => Console.WriteLine("About to Stop process -> {0}", message));
            _channel.On(StopService, message => Console.WriteLine("About to Stop process -> {0}", message));
            _channel.On(ShutDown, message => Console.WriteLine("About to Shut down machine -> {0}", message));
        }

        public void StartApplicationM(string path)
        {
            Application.Start(path);
        }

        public void StopApplicationM(string name)
        {
            Application.Stop(name);
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

        private void ChangePasswordM(string oldPassword, string newPassword)
        {
            Account.Reset(Env.GetUsername(), oldPassword, newPassword);
        }
    }
}