using Microsoft.Extensions.Logging;
using System;
using System.ServiceModel;
using System.ServiceModel.Channels;
using System.ServiceModel.Dispatcher;

namespace Pivotal.RouteServiceIwaWcfInterceptor
{
    public class ImpersonateInterceptor : IClientMessageInspector
    {
        const string CF_IMPERSONATED_IDENTITY_HEADER = "X-Cf-Impersonated-Identity";

        public void AfterReceiveReply(ref Message reply, object correlationState) 
        {

        }

        public object BeforeSendRequest(ref Message request, IClientChannel channel)
        {
            try
            {
                string userId = System.Threading.Thread.CurrentPrincipal.Identity.Name;

                this.Logger().LogDebug($"Executing Impersonation with user '{userId}'");

                HttpRequestMessageProperty httpRequestMessage;
                object httpRequestMessageObject;
                if (request.Properties.TryGetValue(HttpRequestMessageProperty.Name, out httpRequestMessageObject))
                {
                    httpRequestMessage = httpRequestMessageObject as HttpRequestMessageProperty;
                    if (string.IsNullOrEmpty(httpRequestMessage.Headers[CF_IMPERSONATED_IDENTITY_HEADER]))
                    {
                        httpRequestMessage.Headers[CF_IMPERSONATED_IDENTITY_HEADER] = userId;
                    }
                }
                else
                {
                    httpRequestMessage = new HttpRequestMessageProperty();
                    httpRequestMessage.Headers.Add(CF_IMPERSONATED_IDENTITY_HEADER, userId);
                    request.Properties.Add(HttpRequestMessageProperty.Name, httpRequestMessage);
                }

                this.Logger().LogTrace($"Http Header {CF_IMPERSONATED_IDENTITY_HEADER} is set with value '{userId}'");
            }
            catch (Exception exception)
            {
                this.Logger().LogError($"ImpersonateInterceptor error occurred, with exception {exception}");
            }

            return string.Empty;
        }
    }
}