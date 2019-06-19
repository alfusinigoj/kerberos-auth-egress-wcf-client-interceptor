#### Nuget package is available @ https://www.nuget.org/packages/PivotalServices.WcfClient.Kerberos.Interceptor

#### This project offers package to use with a supply buildpack https://github.com/alfusinigoj/route-service-auth-egress-buildpack which helps applying IWA security (kerberos) for app-app/svc-svc secure communication, in PCF. 

============================================================================================================================================
Important Instructions
============================================================================================================================================

1. Add the supply buildpack from "https://github.com/alfusinigoj/route-service-auth-egress-buildpack/releases" in the CF manifest (preferably the latest release). 
   IMPORTANT: Make sure the application is built/published with target platform 'x64'

2. Follow the readme of https://github.com/alfusinigoj/route-service-auth-egress-buildpack to setup the sources for kerberos config and keytab files

   For kerberos config template, please here https://github.com/alfusinigoj/route-service-auth-egress-wcf-client-interceptor/blob/master/src/RouteServiceIwaWcfInterceptor/krb5.ini

3. Set the correct client UPN in AppSettings with key 'ClientUserPrincipalName' as below (this section will be already added by the package)

	<appSettings>
		<add key="ClientUserPrincipalName" value="client_username@domain" />
	</appSettings>

4. Target service UPN has to be provided in the client/endpoint/identity configuration as in the sample below, else will ignore kerberos ticket injection for that endpoint

	<system.serviceModel>
		...
		<client>
		  <endpoint address="http://foo.bar/myservice.svc" 
					binding="basicHttpBinding" 
					bindingConfiguration="BasicHttpBinding" 
					contract="MyService.IService" 
					name="BasicHttpBinding_IService"
					behaviorConfiguration ="myIwaInterceptorBehavior">
					<identity>
						<userPrincipalName value="target_user@domain" />
					</identity>
		  </endpoint>
		</client>
		...
	</system.serviceModel>

5. To see debug logs, please set the log level to "Debug" or "Trace", via environment variable "PivotalIwaWcfClientInterceptor:LogLevel:Default"

Note: 1) The dev packages are available at https://www.myget.org/feed/ajaganathan/package/nuget/PivotalServices.WcfClient.Kerberos.Interceptor
      2) The packages are still in beta version as it still depends on a beta version of GssKerberos package.

============================================================================================================================================
