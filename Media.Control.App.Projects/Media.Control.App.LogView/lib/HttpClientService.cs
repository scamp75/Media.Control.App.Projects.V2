using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Security.Cryptography.X509Certificates;

namespace Media.Control.App.LogView.lib
{
    public class HttpClientService
    {
        public HttpClient CreateClient()
        {
            // SSL 인증서 무시
            var handler = new HttpClientHandler()
            {
                ServerCertificateCustomValidationCallback = (sender, cert, chain, sslPolicyErrors) => true
            };

            return new HttpClient(handler);
        }

    }
}
