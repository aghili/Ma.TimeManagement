using System.Net;

namespace Ma.TimeManagement.Services
{
    public interface ICustomDnsResolver
    {
        IPAddress Resolve(string host);
    }
}