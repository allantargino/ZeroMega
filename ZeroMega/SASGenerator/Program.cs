using Microsoft.ServiceBus;
using System;
using System.Globalization;
using System.Linq;
using System.Net;
using System.Security.Cryptography;
using System.Text;

namespace SASGenerator
{
    public class Program
    {
        static string serviceBusConnectionString = "<INSERT HERE YOUR SERVICE BUS CONNECTION STRING>";

        static void Main(string[] args)
        {
            try
            {
                var token = GetConnectionStringAutoMethod(serviceBusConnectionString);
                //var token = GetConnectionStringManualMethod(serviceBusConnectionString);
                Console.WriteLine(token);
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error! Message: " + ex.Message);
            }
            finally
            {
                Console.ReadLine();
            }
        }

        private static string GetConnectionStringAutoMethod(string serviceBusConnectionString)
        {
            ServiceBusConnectionStringBuilder connectionString = new ServiceBusConnectionStringBuilder(serviceBusConnectionString);

            string ServiceBusNamespace = connectionString.Endpoints.First().Host;
            string namespaceKeyName = connectionString.SharedAccessKeyName;
            string namespaceKey = connectionString.SharedAccessKey;

            // Create a token valid for 180min
            string token = SharedAccessSignatureTokenProvider.GetSharedAccessSignature(namespaceKeyName, namespaceKey, ServiceBusNamespace, TimeSpan.FromMinutes(180));
            return token;
        }

        private static string GetConnectionStringManualMethod(string serviceBusConnectionString)
        {
            var str = new ConnectionStringUtility(serviceBusConnectionString);
            var baseAddress = str.Endpoint;
            var SASKeyValue = str.SasKeyValue;
            var SASKeyName = str.SasKeyName;

            TimeSpan fromEpochStart = DateTime.UtcNow - new DateTime(1970, 1, 1);
            string expiry = Convert.ToString((int)fromEpochStart.TotalSeconds + 3600);

            string stringToSign = WebUtility.UrlEncode(baseAddress) + expiry;

            HMACSHA256 hmac = new HMACSHA256(Encoding.UTF8.GetBytes(SASKeyValue));

            string signature = Convert.ToBase64String(hmac.ComputeHash(Encoding.UTF8.GetBytes(stringToSign)));
            string token = String.Format(CultureInfo.InvariantCulture, "SharedAccessSignature sr={0}&sig={1}&se={2}&skn={3}",
                            WebUtility.UrlEncode(baseAddress), WebUtility.UrlEncode(signature), expiry, SASKeyName);

            return token;
        }

        private class ConnectionStringUtility
        {
            public string Endpoint { get; private set; }
            public string SasKeyName { get; private set; }
            public string SasKeyValue { get; private set; }

            public ConnectionStringUtility(string connectionString)
            {
                //Parse Connectionstring
                char[] separator = { ';' };
                string[] parts = connectionString.Split(separator);
                for (int i = 0; i < parts.Length; i++)
                {
                    if (parts[i].StartsWith("Endpoint"))
                        Endpoint = "https" + parts[i].Substring(11);
                    if (parts[i].StartsWith("SharedAccessKeyName"))
                        SasKeyName = parts[i].Substring(20);
                    if (parts[i].StartsWith("SharedAccessKey"))
                        SasKeyValue = parts[i].Substring(16);
                }
            }
        }
    }

}
