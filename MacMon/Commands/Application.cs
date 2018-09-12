using System;
using System.Diagnostics;

namespace MacMon.Commands
{
    public class Application
    {
        public static bool Start(string path)
        {
            var p = new Process();

            try
            {
                p.StartInfo.FileName = path;
                return p.Start();
            }
            catch (Exception)
            {
                return false;
            }
        }

        public static bool Stop(string name)
        {

            try
            {
                var appsByName = Process.GetProcessesByName(name);
                foreach (var app in appsByName)
                {
                    app.Kill();
                }

                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }
    }
}