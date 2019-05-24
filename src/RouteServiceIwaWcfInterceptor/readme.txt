======================================================================
General Instruction
======================================================================

1. Modify "krb5.ini" file with appropriate information

2. Add the "keytab file" for the user which the caller uses

3. Add the supply buildpack from "https://github.com/macsux/route-service-auth-buildpack/releases" in the CF manifest (preferable the latest one)

4. Make sure you application has the below references added to this project
	  1. System.Configuration
      2. System.IdentityModel
      3. System.ServiceModel
      4. System.ServiceModel.Channels