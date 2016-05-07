using System;
using System.Linq;
using System.Threading.Tasks;
using System.IO;
using System.Net;
using System.Text;
using System.Security.Cryptography.X509Certificates;

namespace Visa {
 public class VisaDirectSample {
  private static string USER_ID = "{put your user id here}"; // Set user ID for App from VDP Portal
  private static string PASSWORD = "{put your password here}"; // Set password for App from VDP Portal

  // P12 File settings
  // Follow instructions in README for generating P12 file
  private static string P12_FILE_PATH = @"{put the path to the P12 file here}";
  private static string P12_FILE_PASSWORD = @"{put the P12 file password}";

  public static void Main (string[] args) {
   string baseUri = @"visadirect/";
   string resourcePath = @"fundstransfer/v1/pullfundstransactions/";
   string url = @"https://sandbox.api.visa.com/" + baseUri + resourcePath;
   string localTransactionDateTime = DateTime.Now.ToString("yyyy-MM-ddTHH:mm:ss");
   string body = "{\"businessApplicationId\":\"AA\",\"foreignExchangeFeeTransaction\":\"1.99\",\"amount\":\"124.02\",\"cardAcceptor\":{\"address\":{\"county\":\"SanMateo\",\"state\":\"CA\",\"zipCode\":\"94404\",\"country\":\"USA\"},\"idCode\":\"ABCD1234ABCD123\",\"name\":\"VisaInc.USA-FosterCity\",\"terminalId\":\"ABCD1234\"},\"senderCurrencyCode\":\"USD\",\"senderCardExpiryDate\":\"2015-10\",\"surcharge\":\"11.99\",\"systemsTraceAuditNumber\":\"451001\",\"acquirerCountryCode\":\"840\",\"acquiringBin\":\"408999\",\"cavv\":\"0700100038238906000013405823891061668252\",\"retrievalReferenceNumber\":\"330000550000\",\"localTransactionDateTime\": \"" + localTransactionDateTime + "\",\"senderPrimaryAccountNumber\":\"4895142232120006\"}";

   string responseBody = makeVisaDirectAPICall(url,
    body,
    USER_ID,
    PASSWORD,
    P12_FILE_PATH,
    P12_FILE_PASSWORD
   );
  }
    
  /// <summary>
  /// Makes a POST call to Visa Direct API.
  /// </summary>
  /// <returns>Response body</returns>
  /// <param name="requestURL">Request URL eg https://sandbox.api.visa.com/visadirect/fundstransfer/v1/pushfundstransactions</param>
  /// <param name="requestBodyString">Request body string.</param>
  /// <param name="userId">userId assigned to your application by VDP. You can get these from the App details on https://vdp.visa.com </param>
  /// <param name="password">password assigned to your application by VDP. You can get these from the App details on https://vdp.visa.com </param>
  /// <param name="certificatePath">Path to p12 file.</param>
  /// <param name="certificatePassword">p12 file password.</param>
  public static string makeVisaDirectAPICall(string requestURL,
   string requestBodyString,
   string userId,
   string password,
   string certificatePath,
   string certificatePassword) {
    // Create the POST request object
    HttpWebRequest request = WebRequest.Create(requestURL) as HttpWebRequest;
    request.Method = "POST";
    request.ContentType = "application/json";
    request.Accept = "application/json";

    // Add headers
    string authString = userId + ":" + password;
    var authStringBytes = System.Text.Encoding.UTF8.GetBytes(authString);
    string authHeaderString = Convert.ToBase64String(authStringBytes);
    request.Headers ["Authorization"] = "Basic " + authHeaderString;

    // Load the body for the post request
    var requestStringBytes = System.Text.Encoding.UTF8.GetBytes(requestBodyString);
    request.GetRequestStream().Write(requestStringBytes, 0, requestStringBytes.Length);

    // Add certificate
    var certificate = new X509Certificate2(certificatePath, certificatePassword);
    request.ClientCertificates.Add(certificate);

    string responseBody = "";
    try {
     // Make the call
     using (HttpWebResponse response = request.GetResponse() as HttpWebResponse) {
      var encoding = ASCIIEncoding.ASCII;
      if (response.StatusCode != HttpStatusCode.OK) {
       throw new Exception (String.Format (
        "Server error.\n\nStatusCode:{0}\n\nStatusDescription:{1}\n\nResponseHeaders:{2}",
        response.StatusCode,
        response.StatusDescription,
        response.Headers.ToString()));
      }
          
      Console.WriteLine("Response Headers: \n" + response.Headers.ToString());          
          
      using (var reader = new StreamReader(response.GetResponseStream(), encoding)) {
       responseBody = reader.ReadToEnd();
      }
          
      Console.WriteLine("Response Body: \n" + responseBody);      
     }
    }
    catch (WebException e) {
     Console.WriteLine(e.Message);
    }
   return responseBody;
  }
 } 
}