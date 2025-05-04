using System.Collections.Generic;
using System.Threading.Tasks;

namespace M_21_31.Logger
{
    public interface IM_21_31_LoggerContext
    {
        void BeginCapture();
        void EndCapture();
        Dictionary<string, string> GetAllRequestHeaders();
        Dictionary<string, string> GetAllResponseHeaders();
        string GetClientIpV4();
        string GetClientIpV6();
        string GetTransactionId();
        double GetElapsedMilliseconds();
        Task<string> GetRequestBody();
        string GetRequestMethod();
        string GetRequestUrl();
        Task<string> GetResponseBody();
        string GetResponseStatusCode();
        string GetServerIp();
        string GetServerMacAddress();
        string GetUserName();
    }
}