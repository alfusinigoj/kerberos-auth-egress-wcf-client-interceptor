using Microsoft.AspNetCore.Authentication.GssKerberos;
using System;
using System.Diagnostics;
using System.IO;
using System.ServiceModel;
using System.ServiceModel.Channels;
using System.ServiceModel.Dispatcher;
using System.Text.RegularExpressions;

namespace Pivotal.RouteServiceIwaWcfInterceptor
{
    public class SvcRequestIwaInterceptor : IClientMessageInspector
    {
        const string CONTAINER_APP_BIN_PATH = @"C:\Users\vcap\app\bin";
        private const string AUTHORIZATION_HEADER = "Authorization";

        public SvcRequestIwaInterceptor()
        {
        }

        #region IClientMessageInspector Members
        public void AfterReceiveReply(ref Message reply, object correlationState)
        {

        }

        public object BeforeSendRequest(ref Message request, IClientChannel channel)
        {
            var clientUpn = (string)channel.RemoteAddress.Identity.IdentityClaim.Resource;

            if (string.IsNullOrWhiteSpace(clientUpn))
                throw new Exception($"No identity/userPrincipalName set for the endpoint '{channel.RemoteAddress.Uri.OriginalString}'");

            var spn = $"host/{channel.RemoteAddress.Uri.Host}";

            HttpRequestMessageProperty httpRequestMessage;
            object httpRequestMessageObject;
            if (request.Properties.TryGetValue(HttpRequestMessageProperty.Name, out httpRequestMessageObject))
            {
                httpRequestMessage = httpRequestMessageObject as HttpRequestMessageProperty;
                if (string.IsNullOrEmpty(httpRequestMessage.Headers[AUTHORIZATION_HEADER]))
                {
                    httpRequestMessage.Headers[AUTHORIZATION_HEADER] = GetKerberosTicket(spn, clientUpn);
                }
            }
            else
            {
                httpRequestMessage = new HttpRequestMessageProperty();
                httpRequestMessage.Headers.Add(AUTHORIZATION_HEADER, GetKerberosTicket(spn, clientUpn));
                request.Properties.Add(HttpRequestMessageProperty.Name, httpRequestMessage);
            }
            return null;
        }
        private string GetKerberosTicket(string spn, string clientUpn)
        {
            EnsureTgt(clientUpn);

            using (var clientCredentials = GssCredentials.FromKeytab(clientUpn, CredentialUsage.Initiate))
            {
                using (var initiator = new GssInitiator(credential: clientCredentials, spn: spn))
                {
                    try
                    {
                        var kerberosTicket = Convert.ToBase64String(initiator.Initiate(null));
                        Console.WriteLine($"Ticket: {kerberosTicket}");
                        return $"Negotiate {kerberosTicket}";
                    }
                    catch (GssException exception)
                    {
                        Console.Error.WriteLine(exception.Message);
                        return string.Empty;
                    }
                }
            }
        }
        #endregion

        private static void EnsureTgt(string principal)
        {
            var expiry = GetTgtExpiry();
            if (expiry < DateTime.Now)
                ObtainTgt(principal);
        }

        private static DateTime GetTgtExpiry()
        {
            try
            {
                var klistResult = RunCmd(Path.Combine(CONTAINER_APP_BIN_PATH, "klist.exe"), null);
                var tgtExpiryMatch = Regex.Match(klistResult, ".{17}(?=  krbtgt)");
                if (tgtExpiryMatch.Success && DateTime.TryParse(tgtExpiryMatch.Value, out var expiry))
                    return expiry;
            }
            catch (Exception ex)
            {
                Console.Error.WriteLine(ex.Message);
            }

            return DateTime.MinValue;
        }
        private static void ObtainTgt(string principal)
        {
            RunCmd(Path.Combine(CONTAINER_APP_BIN_PATH, "kinit.exe"), $"-k -i {principal}");
        }

        private static string RunCmd(string cmd, string args)
        {
            var cmdsi = new ProcessStartInfo(cmd);
            cmdsi.Arguments = args;
            cmdsi.RedirectStandardOutput = true;
            cmdsi.RedirectStandardError = true;
            cmdsi.UseShellExecute = false;
            var proc = Process.Start(cmdsi);
            var result = proc.StandardOutput.ReadToEnd();
            var err = proc.StandardError.ReadToEnd();
            if (!string.IsNullOrWhiteSpace(err)) throw new Exception(err);
            proc.WaitForExit();
            return result;
        }
    }
}