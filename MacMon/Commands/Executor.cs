using System;
using System.Collections.Generic;
using Phoenix;

namespace MacMon.Commands
{
    public class Executor
    {
        private readonly Channel _channel;

        private const string ChangePassword = "CHANGE_PASSWORD";
        private const string ChangePasswordDone = "CHANGE_PASSWORD_DONE";
        private const string StartApplication = "START_APPLICATION";
        private const string StartService = "START_SERVICE";
        private const string StopApplication = "STOP_APPLICATION";
        private const string StopService = "STOP_SERVICE";
        private const string ShutDown = "SHUT_DOWN";
        private const string Restart = "RESTART";

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
            _channel.On(ChangePassword, message =>
                {
                    var oldPassword = (string)message.payload["old_password"];
                    var newPassword = (string)message.payload["new_password"];
                    try
                    {
                        ChangePasswordM(oldPassword, newPassword);
                        var data = new Dictionary<string, object> {{"success", true}};
                        _channel.Push(ChangePasswordDone, data);
                    }
                    catch (Exception)
                    {
                        var data = new Dictionary<string, object> {{"success", false}};
                        _channel.Push(ChangePasswordDone, data);
                    }
                    Console.WriteLine("About to change password -> {0}", message);
                }
            );
            
            _channel.On(StartApplication, message =>
            {
                Console.WriteLine("About to Start process -> {0}", message);
                var path = (string) message.payload["path"];
                StartApplicationM(path);
            });
            
            _channel.On(StopApplication, message =>
            {
                Console.WriteLine("About to Start process -> {0}", message);
                var name = (string) message.payload["name"];
                StopApplicationM(name);
            });
            
            _channel.On(StartService, message =>
            {
                Console.WriteLine("About to Stop process -> {0}", message);
                var name = (string) message.payload["name"];
                StartServiceM(name);
            });
            
            _channel.On(StopService, message =>
            {
                Console.WriteLine("About to Stop process -> {0}", message);
                var name = (string) message.payload["name"];
                StopServiceM(name);
            });
            
            _channel.On(ShutDown, message =>
            {
                Console.WriteLine("About to Shut down machine -> {0}", message);
                ShutDownM();
            });
            
            _channel.On(Restart, message =>
            {
                Console.WriteLine("About to Shut down machine -> {0}", message);
                RestartM();
            });
        }

        private static void StartApplicationM(string path)
        {
            Application.Start(path);
        }

        private static void StopApplicationM(string name)
        {
            Application.Stop(name);
        }

        private static void StartServiceM(string name)
        {
            Service.Start(name);
        }

        private static void StopServiceM(string name)
        {
            Service.Stop(name);
        }

        private static void ShutDownM()
        {
            Power.Off();
        }
        
        private static void RestartM()
        {
            Power.Restart();
        }

        private static void ChangePasswordM(string oldPassword, string newPassword)
        {
            Account.Reset(Env.GetUsername(), oldPassword, newPassword);
        }
    }
}