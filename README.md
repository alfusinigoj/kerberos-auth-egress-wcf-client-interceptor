### PivotalServices.WcfClient.Kerberos.Interceptor

Stable versions are available at www.nuget.org (please refer to the note at the end for more details)

This package will add a Wcf client interceptor which will injects kerberos ticket for egress requests. This should be used together with the supply buildpack https://github.com/macsux/route-service-auth-buildpack/releases

#### Below are the important developer instructions, to follow after installation of this package

1. Modify "krb5.ini" file with appropriate values (the one loaded now is just a template)
2. Add the "keytab file" for the user which the wcf client uses to connect to the service, make sure to set the properties to "Copy Always" with build action "None"
3. Add the supply buildpack from "https://github.com/macsux/route-service-auth-buildpack/releases" in the CF manifest (preferably the latest release)
4. Set the kerberos configuration file using environment variable 'KRB5_CONFIG' in the app manifest. for e.g KRB5_CONFIG: "C:\Users\vcap\app\krb5.ini", if not it liios into the bin directory by default
5. Set the app bin path using the environment variable 'APP_BIN_PATH', if not the default path used is 'C:\Users\vcap\app\bin'
6. Set the correct client UPN in AppSettings with key 'ClientUserPrincipalName' as below (this section will be already added by the package)
```xml
  <appSettings>
		<add key="ClientUserPrincipalName" value="client_username@domain" />
  </appSettings>
```
7. Target service UPN has to be provided in the client/endpoint/identity configuration as in the sample below. If not, system will try to use the SPN 'host/foo.bar' (based on the below sample)
```xml
  <system.serviceModel>
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
	</system.serviceModel>
  ```
8. To see debug logs, please set the log level to "Debug" or "Trace", via environment variable "PivotalIwaWcfClientInterceptor:LogLevel:Default" 

##### Notes
1. The dev/alpha packages are available at https://www.myget.org/feed/ajaganathan/package/nuget/Pivotal.WcfClient.Kerberos.Interceptor, feed https://www.myget.org/F/ajaganathan/api/v3/index.json
2. The packages are still in beta version as it still depends on a beta version of GssKerberos package.
