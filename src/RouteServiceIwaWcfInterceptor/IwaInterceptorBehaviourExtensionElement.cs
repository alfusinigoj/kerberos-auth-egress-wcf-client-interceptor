using System;
using System.ServiceModel.Configuration;

namespace Pivotal.RouteServiceIwaWcfInterceptor
{
    public class MyIwaInterceptorBehaviourExtensionElement : BehaviorExtensionElement
    {
        public override Type BehaviorType => typeof(IwaInterceptorEndpointBehaviour);

        protected override object CreateBehavior()
        {
            return new IwaInterceptorEndpointBehaviour();
        }
    }
}