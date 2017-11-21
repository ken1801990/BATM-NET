using BitcoinATM_Application.TransferObject;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace BitcoinATM_Application.Service
{
    public static class ServiceCommunicate
    {
        private const string RegionName = "us-west-2"; //This is the regionName
        private const string ServiceName = "apigateway";
        private const string Algorithm = "AWS4-HMAC-SHA256";
        private const string ContentType = "text/plain;charset=UTF-8";
        private const string Host = "apigateway.eu-west-1.amazonaws.com";
        private const string SignedHeaders = "content-type;host;x-amz-date";
        private const string SecretKey = "QHf0s9dcG2YKNH8rifMU9fAY4fPFiVR1FcKw+wi3";
        private const string AccessKey = "AKIAIUGKLNPNLQME5JXA";
        private const string Uri = "https://uxcgdemlml.execute-api.us-west-2.amazonaws.com/GatewayService";
        public static WebRequest RequestGet(string canonicalQueriString, string jsonString)
        {
            string hashedRequestPayload = CreateRequestPayload("");

            string authorization = Sign(hashedRequestPayload, "GET", Uri, canonicalQueriString);
            string requestDate = DateTime.UtcNow.ToString("yyyyMMddTHHmmss") + "Z";

            WebRequest webRequest = WebRequest.Create(Uri);

            webRequest.Method = "GET";
            webRequest.ContentType = ContentType;
            webRequest.Headers.Add("X-Amz-date", requestDate);
            webRequest.Headers.Add("Authorization", authorization);
            webRequest.Headers.Add("x-amz-content-sha256", hashedRequestPayload);
            return webRequest;
        }

        public static ResponseData RequestPost(string jsonString)
        {
            string hashedRequestPayload = CreateRequestPayload(jsonString);

            string authorization = Sign(hashedRequestPayload, "POST", Uri, string.Empty);
            string requestDate = DateTime.UtcNow.ToString("yyyyMMddTHHmmss") + "Z";

            WebRequest webRequest = WebRequest.Create(Uri);

            webRequest.Timeout = 20000;
            webRequest.Method = "POST";
            webRequest.ContentType = ContentType;
            webRequest.Headers.Add("X-Amz-date", requestDate);
            webRequest.Headers.Add("Authorization", authorization);
            webRequest.Headers.Add("x-amz-content-sha256", hashedRequestPayload);
            webRequest.ContentLength = jsonString.Length;

            ASCIIEncoding encoding = new ASCIIEncoding();
            byte[] data = encoding.GetBytes(jsonString);

            Stream newStream = webRequest.GetRequestStream();
            newStream.Write(data, 0, data.Length);

            var resultStr = "";
            using (var reader = new StreamReader(webRequest.GetResponse().GetResponseStream()))
            {
                resultStr = reader.ReadToEnd();
            }
            var dataReturnStr = JsonConvert.DeserializeObject<dynamic>(resultStr);
            var dataReturn = JsonConvert.DeserializeObject<ResponseData>(dataReturnStr);
            return dataReturn;
        }
        private static string CreateRequestPayload(string jsonString)
        {
            //Here should be JSON object of the model we are sending with POST request
            //var jsonToSerialize = new { Data = String.Empty };

            //We parse empty string to the serializer if we are makeing GET request
            //string requestPayload = new JavaScriptSerializer().Serialize(jsonToSerialize);
            string hashedRequestPayload = HexEncode(Hash(ToBytes(jsonString)));
            return hashedRequestPayload;
        }

        private static string Sign(string hashedRequestPayload, string requestMethod, string canonicalUri, string canonicalQueryString)
        {
            var currentDateTime = DateTime.UtcNow;
            var accessKey = AccessKey; //Here place your app ACCESS_KEY
            var secretKey = SecretKey; //Here is a place for you app SECRET_KEY

            var dateStamp = currentDateTime.ToString("yyyyMMdd");
            var requestDate = currentDateTime.ToString("yyyyMMddTHHmmss") + "Z";
            var credentialScope = string.Format("{0}/{1}/{2}/aws4_request", dateStamp, RegionName, ServiceName);

            var headers = new SortedDictionary<string, string> {
            { "content-type", ContentType },
            { "host", Host  },
            { "x-amz-date", requestDate }
        };

            string canonicalHeaders = string.Join("\n", headers.Select(x => x.Key.ToLowerInvariant() + ":" + x.Value.Trim())) + "\n";

            // Task 1: Create a Canonical Request For Signature Version 4
            string canonicalRequest = requestMethod + "\n" + canonicalUri + "\n" + canonicalQueryString + "\n" + canonicalHeaders + "\n" + SignedHeaders + "\n" + hashedRequestPayload;
            string hashedCanonicalRequest = HexEncode(Hash(ToBytes(canonicalRequest)));

            // Task 2: Create a String to Sign for Signature Version 4
            string stringToSign = Algorithm + "\n" + requestDate + "\n" + credentialScope + "\n" + hashedCanonicalRequest;

            // Task 3: Calculate the AWS Signature Version 4
            byte[] signingKey = GetSignatureKey(secretKey, dateStamp, RegionName, ServiceName);
            string signature = HexEncode(HmacSha256(stringToSign, signingKey));

            // Task 4: Prepare a signed request
            // Authorization: algorithm Credential=access key ID/credential scope, SignedHeadaers=SignedHeaders, Signature=signature

            string authorization = string.Format("{0} Credential={1}/{2}/{3}/{4}/aws4_request, SignedHeaders={5}, Signature={6}",
            Algorithm, accessKey, dateStamp, RegionName, ServiceName, SignedHeaders, signature);

            return authorization;
        }

        private static byte[] GetSignatureKey(string key, string dateStamp, string regionName, string serviceName)
        {
            byte[] kDate = HmacSha256(dateStamp, ToBytes("AWS4" + key));
            byte[] kRegion = HmacSha256(regionName, kDate);
            byte[] kService = HmacSha256(serviceName, kRegion);
            return HmacSha256("aws4_request", kService);
        }

        private static byte[] ToBytes(string str)
        {
            return Encoding.UTF8.GetBytes(str.ToCharArray());
        }

        private static string HexEncode(byte[] bytes)
        {
            return BitConverter.ToString(bytes).Replace("-", string.Empty).ToLowerInvariant();
        }

        private static byte[] Hash(byte[] bytes)
        {
            return SHA256.Create().ComputeHash(bytes);
        }

        private static byte[] HmacSha256(string data, byte[] key)
        {
            return new HMACSHA256(key).ComputeHash(ToBytes(data));
        }
    }
}