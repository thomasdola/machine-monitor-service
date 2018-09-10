using System.Management;

namespace MacMon.Commands
{
    public class Power
    {
        public static void Off()
        {
            System.Diagnostics.Process.Start("shutdown", "/s /t 0");
        }

        public static void Restart()
        {
            System.Diagnostics.Process.Start("shutdown", "/r /t 0");
        }
    }
}