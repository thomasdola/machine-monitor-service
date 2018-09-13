using System;
using System.Diagnostics;

namespace MacMon.Commands
{
    public static class Application
    {
        public static void Start(string path)
        {
            var p = new Process();

            try
            {
                p.StartInfo.FileName = path;
                p.Start();
            }
            catch (Exception)
            {
                // ignored
            }
        }

        public static void Stop(string name)
        {

            try
            {
                var appsByName = Process.GetProcessesByName(name);
                foreach (var app in appsByName)
                {
                    app.Kill();
                }
            }
            catch (Exception)
            {
                // ignored
            }
        }
    }
}