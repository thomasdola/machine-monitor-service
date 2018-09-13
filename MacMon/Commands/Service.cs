using System.Linq;
using System.ServiceProcess;

namespace MacMon.Commands
{
    public static class Service
    {
        public static void Start(string name)
        {
            if (!IsInstalled(name)) return;

            var sc = GetService(name);
            if (!sc.Status.Equals(ServiceControllerStatus.Stopped) &&
                !sc.Status.Equals(ServiceControllerStatus.StopPending))
                return;
            sc.Start();
            sc.WaitForStatus(ServiceControllerStatus.Running);
        }

        public static void Stop(string name)
        {
            if (!IsInstalled(name)) return;
            var sc = GetService(name);
            if (sc.Status.Equals(ServiceControllerStatus.Stopped) &&
                sc.Status.Equals(ServiceControllerStatus.StopPending))
                return;
            sc.Stop(); 
            sc.WaitForStatus(ServiceControllerStatus.Stopped);
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