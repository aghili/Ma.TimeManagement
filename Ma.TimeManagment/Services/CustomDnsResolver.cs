using Microsoft.Extensions.Configuration;
using System.Net;

namespace Ma.TimeManagement.Services
{
    public class CustomDnsResolver : ICustomDnsResolver
    {
        private readonly Dictionary<string, IPAddress> _map = [];

        public CustomDnsResolver(IConfiguration config)
        {
            //_map = config.GetSection("DnsMap")
            //             .GetChildren()
            //             .ToDictionary(
            //                 x => x.Key,
            //                 x => IPAddress.Parse(x.Value));
            _map.Add("feed-srv.mhd.mahaksoft.com", IPAddress.Parse("192.168.0.52"));
        }

        public IPAddress Resolve(string host)
        {
            var clean_host = host.Split(':')[0];
            if (_map.ContainsKey(clean_host))
            {
             return   _map[clean_host];
            }
            return Dns.GetHostAddresses(clean_host).First();
        }
    }
}
