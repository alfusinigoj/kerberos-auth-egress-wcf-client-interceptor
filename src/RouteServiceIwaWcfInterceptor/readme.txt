============================================================================================================================================
Important Instructions
============================================================================================================================================

Note: This package is installed from https://www.myget.org/feed/ajaganathan/package/nuget/Pivotal.WcfClient.Kerberos.Interceptor

1. Modify "krb5.ini" file with appropriate values (the one loaded now is just a template)

2. Add the "keytab file" for the user which the wcf client uses to connect to the service, make sure to set the properties to "Copy Always" with build action "None"

3. Add the supply buildpack from "https://github.com/macsux/route-service-auth-buildpack/releases" in the CF manifest (preferably the latest release)

4. Set the kerberos configuration file using environment variable 'KRB5_CONFIG', if not the default path used is 'C:\Users\vcap\app\krb5.ini'

5. Set the app bin path using the environment variable 'APP_BIN_PATH', if not the default path used is 'C:\Users\vcap\app\bin'

6. SPN will be automatically resolved from the url. For. e.g, if the svc endpoint is 'http://foo.bar/myservice.svc', the SPN resolved will be 'host/foo.bar'

7. Client's UPN has to be provided in the client/endpoint/identity configuration as in the sample below.

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
						<userPrincipalName value="abc@mydomain.com" />
					</identity>
		  </endpoint>
		</client>
		...
	</system.serviceModel>

8. To see debug logs, please set the log level to "Debug" or "Trace", via environment variable "PivotalIwaWcfClientInterceptor:LogLevel:Default"

============================================================================================================================================
