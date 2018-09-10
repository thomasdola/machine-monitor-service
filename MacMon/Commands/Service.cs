using System.Linq;
using System.ServiceProcess;

namespace MacMon.Commands
{
    public class Service
    {
        public static bool Start(string name)
        {
            if (!IsInstalled(name)) return false;
            
            var sc = GetService(name);
            if (!sc.Status.Equals(ServiceControllerStatus.Stopped) &&
                !sc.Status.Equals(ServiceControllerStatus.StopPending)) return false;
            sc.Start();
            sc.WaitForStatus(ServiceControllerStatus.Running);
            return true;

        }

        public static bool Stop(string name)
        {
            if (!IsInstalled(name)) return false;
            var sc = GetService(name);
            if (sc.Status.Equals(ServiceControllerStatus.Stopped) &&
                sc.Status.Equals(ServiceControllerStatus.StopPending)) return false;
            sc.Stop(); 
            sc.WaitForStatus(ServiceControllerStatus.Stopped);
            return true;

        }

        private static bool IsInstalled(string name)
        {
            var sc = GetService(name);
            return sc != null;
        }

        private static ServiceController GetService(string name)
        {
            return ServiceController.GetServices()
                .FirstOrDefault(s => s.ServiceName == name);
        }
    }
}