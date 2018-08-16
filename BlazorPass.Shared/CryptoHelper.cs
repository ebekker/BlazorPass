using Microsoft.AspNetCore.WebUtilities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace BlazorPass.Shared
{
    public class CryptoHelper : IDisposable
    {
        RSA _rsa;

        public CryptoHelper(string json)
        {
            _rsa = RSA.Create();
            if (!string.IsNullOrEmpty(json))
                _rsa.FromQueryString(json);
        }

        public string ExportPublic => _rsa.ToQueryString(false);

        public string ExportPrivate => _rsa.ToQueryString(true);

        public string Encrypt(string clear)
        {
            var clearBytes = Encoding.UTF8.GetBytes(clear);
            return EncodeBase64U(_rsa.Encrypt(clearBytes, RSAEncryptionPadding.Pkcs1));
        }

        public string Decrypt(string crypt)
        {
            var cryptBytes = DecodeBase64U(crypt);
            return Encoding.UTF8.GetString(_rsa.Decrypt(cryptBytes, RSAEncryptionPadding.Pkcs1));
        }

        public static string EncodeBase64U(byte[] value) =>
            Convert.ToBase64String(value)
                .Replace("+", "-")
                .Replace("/", "_")
                .Replace("=", ".");

        public static byte[] DecodeBase64U(string value) =>
            Convert.FromBase64String(value
                .Replace(".", "=")
                .Replace("_", "/")
                .Replace("-", "+"));

        public void Dispose()
        {
            _rsa?.Dispose();
            _rsa = null;
        }
    }

    // Adapted from:
    //    https://github.com/dotnet/core/issues/874#issuecomment-323894072
    public static class RsaExtensions
    {
        public static string ToString(byte[] bytes)
        {
            if (bytes == null)
                return string.Empty;
            return Convert.ToBase64String(bytes);
        }

        public static byte[] ToBytes(string base64)
        {
            if (string.IsNullOrEmpty(base64))
                return null;
            return Convert.FromBase64String(base64);
        }

        public static byte[] ToBytes(
                Dictionary<string, Microsoft.Extensions.Primitives.StringValues> d, string key)
        {
            if (d.TryGetValue(key, out var values))
                return ToBytes(values.ToString());
            return null;
        }

        public static string ToQueryString(this RSA rsa, bool includePrivateParameters)
        {
            RSAParameters parameters = rsa.ExportParameters(includePrivateParameters);

            return QueryHelpers.AddQueryString("rsa", new Dictionary<string, string>
            {
                ["Mo"] = ToString(parameters.Modulus),
                ["Ex"] = ToString(parameters.Exponent),
                ["P"] = ToString(parameters.P),
                ["Q"] = ToString(parameters.Q),
                ["DP"] = ToString(parameters.DP),
                ["DQ"] = ToString(parameters.DQ),
                ["IQ"] = ToString(parameters.InverseQ),
                ["D"] = ToString(parameters.D),
            });
        }

        internal static void FromQueryString(this RSA rsa, string query)
        {
            if (query.StartsWith("rsa?"))
                query = query.Substring(4);

            RSAParameters parameters = new RSAParameters();

            var queryParams = QueryHelpers.ParseQuery(query);

            foreach (var k in queryParams.Keys)
                Console.WriteLine("  * GOT: " + k + queryParams[k]);

            parameters.Modulus = ToBytes(queryParams, "Mo");
            parameters.Exponent = ToBytes(queryParams, "Ex");
            parameters.P = ToBytes(queryParams, "P");
            parameters.Q = ToBytes(queryParams, "Q");
            parameters.DP = ToBytes(queryParams, "DP");
            parameters.DQ = ToBytes(queryParams, "DQ");
            parameters.InverseQ = ToBytes(queryParams, "IQ");
            parameters.D = ToBytes(queryParams, "D");

            rsa.ImportParameters(parameters);
        }
    }
}
