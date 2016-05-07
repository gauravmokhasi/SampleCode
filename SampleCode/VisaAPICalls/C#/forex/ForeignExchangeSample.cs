using System;
using System.Linq;
using System.Threading.Tasks;
using System.IO;
using System.Net;
using System.Text;
using System.Security.Cryptography.X509Certificates;

namespace Visa
{
    public class ForeignExchangeSample
    {
        private static string USER_ID = "{put your user id here}"; // Set user ID for App from VDP Portal
        private static string PASSWORD = "{put your password here}"; // Set password for App from VDP Portal

        // P12 File settings
        // Follow instructions in README for generating P12 file
        private static string P12_FILE_PATH = @"{put the path to the P12 file here}";
        private static string P12_FILE_PASSWORD = @"{put the P12 file password}";

        public static void Main(string[] args)
        {
            string resourcePath = @"forexrates/v1/foreignexchangerates";
            string url = @"https://sandbox.api.visa.com/" + resourcePath;
            // Load the body for the post request
            string body = "{\"acquirerCountryCode\": \"840\", \"acquiringBin\": \"408999\", \"cardAcceptor\":{\"address\": {\"city\": \"Foster City\",\"country\": \"RU\",\"county\": \"San Mateo\",\"state\": \"CA\",\"zipCode\": \"94404\"}, \"idCode\": \"ABCD1234ABCD123\",\"name\": \"ABCD\", \"terminalId\": \"ABCD1234\" }, \"destinationCurrencyCode\": \"840\", \"markUpRate\": \"1\",\"retrievalReferenceNumber\": \"201010101031\",\"sourceAmount\": \"100\",\"sourceCurrencyCode\": \"643\",\"systemsTraceAuditNumber\": \"350421\"}";
            string responseBody = MakeForexApiCall(url, body, USER_ID, PASSWORD, P12_FILE_PATH, P12_FILE_PASSWORD);
        }

        /// <summary>
        /// Makes a POST call to Foreign Exchange API.
        /// </summary>
        /// <returns>Response body</returns>
        /// <param name="requestURL">Request URL eg https://sandbox.api.visa.com/forexrates/v1/foreignexchangerates </param>
        /// <param name="requestBodyString">Request body string.</param>
        /// <param name="userId">userId assigned to your application by VDP. You can get these from the App details on https://vdp.visa.com </param>
        /// <param name="password">password assigned to your application by VDP. You can get these from the App details on https://vdp.visa.com </param>
        /// <param name="certificatePath">Path to p12 file.</param>
        /// <param name="certificatePassword">p12 file password.</param>
        public static string MakeForexApiCall(string requestURL, string requestBodyString, string userId, string password, string certificatePath, string certificatePassword)
        {
            // Create the POST request object
            HttpWebRequest request = WebRequest.Create(requestURL) as HttpWebRequest;
            request.Method = "POST";
            request.ContentType = "application/json";
            request.Accept = "application/json";

            // Add headers
            string authString = userId + ":" + password;
            var authStringBytes = System.Text.Encoding.UTF8.GetBytes(authString);
            string authHeaderString = Convert.ToBase64String(authStringBytes);
            request.Headers["Authorization"] = "Basic " + authHeaderString;

            // Load the body for the post request
            var requestStringBytes = System.Text.Encoding.UTF8.GetBytes(requestBodyString);
            request.GetRequestStream().Write(requestStringBytes, 0, requestStringBytes.Length);

            // Add certificate
            var certificate = new X509Certificate2(certificatePath, certificatePassword);
            request.ClientCertificates.Add(certificate);

            string responseBody = "";
            try
            {
                // Make the call
                using (HttpWebResponse response = request.GetResponse() as HttpWebResponse)
                {
                    var encoding = ASCIIEncoding.ASCII;
                    if (response.StatusCode != HttpStatusCode.OK)
                    {
                        throw new Exception(String.Format(
                         "Server error.\n\nStatusCode:{0}\n\nStatusDescription:{1}\n\nResponseHeaders:{2}",
                         response.StatusCode,
                         response.StatusDescription,
                         response.Headers.ToString()));
                    }

                    Console.WriteLine("Response Headers: \n" + response.Headers.ToString());

                    using (var reader = new StreamReader(response.GetResponseStream(), encoding))
                    {
                        responseBody = reader.ReadToEnd();
                    }

                    Console.WriteLine("Response Body: \n" + responseBody);
                }
            }
            catch (WebException e)
            {
                Console.WriteLine(e.Message);
            }
            return responseBody;
        }
    }
}