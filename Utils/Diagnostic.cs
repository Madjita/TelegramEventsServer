using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace Utils
{
    public static class Diagnostic
    {
        public static string ConcatenatedIPs => GetConcatenatedIPs();

        private static string GetConcatenatedIPs()
        {
            string hostName = Dns.GetHostName();
            IPHostEntry ipEntry = Dns.GetHostEntry(hostName);

            string[] ipv4Addresses = ipEntry.AddressList
                .Where(ip => ip.AddressFamily == System.Net.Sockets.AddressFamily.InterNetwork) // Выбираем только IPv4
                .Select(ip => ip.ToString())
                .ToArray();
            return string.Join(Environment.NewLine, ipv4Addresses);
        }
    }
}
