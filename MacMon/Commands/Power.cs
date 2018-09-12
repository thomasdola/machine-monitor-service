using System.Diagnostics;

namespace MacMon.Commands
{
    public class Power
    {
        public static void Off()
        {
            Process.Start("shutdown", "/s /t 0");
        }

        public static void Restart()
        {
            Process.Start("shutdown", "/r /t 0");
        }
    }
}