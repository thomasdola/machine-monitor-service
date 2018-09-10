using System.Collections.Generic;
using System.Net.NetworkInformation;

namespace MacMon.Network
{
    public static class Connection
    {
        public static bool IsAvailable()
        {
            return NetworkInterface.GetIsNetworkAvailable();
        }
    }
    
    public class Monitor
    {
        private readonly List<NetworkAvailabilityChangedEventHandler> _handlers;

        private Monitor(List<NetworkAvailabilityChangedEventHandler> handlers)
        {
            _handlers = handlers;
        }

        public static void Init(List<NetworkAvailabilityChangedEventHandler> handlers)
        {
            new Monitor(handlers).Start();
        }

        private void Start()
        {
            foreach (var handler in _handlers)
            {
                NetworkChange.NetworkAvailabilityChanged += handler;
            }
        }
    }
}