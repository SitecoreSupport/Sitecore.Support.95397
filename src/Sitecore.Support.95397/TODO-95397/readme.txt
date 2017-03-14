1. If the "AutofacContrib.CommonServiceLocator.dll" assembly is not present in the website\bin folder, copy one.
2. If the following error is shown on the solution startup:

Could not load file or assembly 'AutofacContrib.CommonServiceLocator, Version=2.6.3.862, Culture=neutral, PublicKeyToken=17863af14b0044da' or one of its dependencies.

configure the assembly dependency in the web.config file to use the "2.6.3.862" version:

<runtime>
    <assemblyBinding xmlns="urn:schemas-microsoft-com:asm.v1">
	. . .
	
      <dependentAssembly>
        <assemblyIdentity name="AutofacContrib.CommonServiceLocator" publicKeyToken="17863af14b0044da" xmlns="urn:schemas-microsoft-com:asm.v1" />
        <bindingRedirect oldVersion="1.0.0.0-2.6.3.862" newVersion="2.6.3.862" xmlns="urn:schemas-microsoft-com:asm.v1" />
      </dependentAssembly>
	  
    </assemblyBinding>
</runtime>

3. Delete the "website\TODO-95397" folder.