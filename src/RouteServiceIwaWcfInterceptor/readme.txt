============================================================================================================================================
Important Instructions
============================================================================================================================================

1. Modify "krb5.ini" file with appropriate values (the one loaded now is just a template)

2. Add the "keytab file" for the user which the wcf client uses to connect to the service

3. Add the supply buildpack from "https://github.com/alfusinigoj/route-service-auth-egress-buildpack/releases" in the CF manifest (preferably the latest release). 
   IMPORTANT: Make sure the application is built/published with target platform 'x64'

4. Set the kerberos configuration file using environment variable 'KRB5_CONFIG' in the app manifest. for e.g KRB5_CONFIG: "C:\Users\vcap\app\krb5.ini", if not it looks into the bin directory by default

5. Set the app bin path using the environment variable 'APP_BIN_PATH', if not the default path used is 'C:\Users\vcap\app\bin'

6. Set the correct client UPN in AppSettings with key 'ClientUserPrincipalName' as below (this section will be already added by the package)

	<appSettings>
		<add key="ClientUserPrincipalName" value="client_username@domain" />
	</appSettings>

7. Target service UPN has to be provided in the client/endpoint/identity configuration as in the sample below, else will ignore kerberos ticket injection for that endpoint

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

8. To see debug logs, please set the log level to "Debug" or "Trace", via environment variable "PivotalIwaWcfClientInterceptor:LogLevel:Default"

Note: 1) The dev packages are available at https://www.myget.org/feed/ajaganathan/package/nuget/PivotalServices.WcfClient.Kerberos.Interceptor
      2) The packages are still in beta version as it still depends on a beta version of GssKerberos package.

============================================================================================================================================
