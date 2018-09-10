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

        public static bool Stop(string path)
        {
            var p = new Process();

            try
            {
                p.StartInfo.FileName = path;
                p.Kill();
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }
    }
}