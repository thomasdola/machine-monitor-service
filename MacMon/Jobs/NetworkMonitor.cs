using System;
using System.Collections.Generic;
using System.Net.NetworkInformation;
using System.Net.Sockets;
using System.Threading.Tasks;
using Phoenix;
using Quartz;

namespace MacMon.Jobs
{
    public class NetworkMonitor  : IJob
    {
        private static void Watch(Channel channel)
        {
            var adapters = NetworkInterface.GetAllNetworkInterfaces();
            foreach (var adapter in adapters)
            {
                var ipv4Info = adapter.GetIPv4Statistics();
                var ipProps = adapter.GetIPProperties();
                
                var report = new Dictionary<string, object>
                {
                    { "id", adapter.Id },
                    { "is_receive_only", adapter.IsReceiveOnly },
                    { "name", adapter.Name },
                    { "description", adapter.Description },
                    { "type", adapter.NetworkInterfaceType },
                    { "status", adapter.OperationalStatus },
                    { "address", adapter.GetPhysicalAddress() },
                    { "speed", adapter.Speed },
                    { "supports_multicast", adapter.SupportsMulticast },
                    { "output_queue_length", ipv4Info.OutputQueueLength },
                    { "bytes_received", ipv4Info.BytesReceived },
                    { "bytes_sent", ipv4Info.BytesSent }
                };

                foreach (var ip in ipProps.UnicastAddresses)
                {
                    if (adapter.OperationalStatus == OperationalStatus.Up
                        && ip.Address.AddressFamily == AddressFamily.InterNetwork)
                    {
                        report.Add("ip_address", ip.Address);
                        
                        Console.WriteLine("IP Address -> {0}", ip.Address);
                    }
                }
                if (channel.canPush)
                {
                    channel.Push(Services.WebSocket.MacMonWebSocket.NetworkStatusChanged, report);
                }
            }
        }

        public async Task Execute(IJobExecutionContext context)
        {
            var channel = (Channel) context.JobDetail.JobDataMap["channel"];
            Watch(channel);
        }
    }
}