# Capgemini Package Deployer Template

The Capgemini Package Deployer template provides a base Package Deployer package template class with additional functionality when deploying packages with the Package Deployer. 


## Table of Contents

- [Installation](#Installation)
- [Usage](#Usage)
- [Contributing](#Contributing)
- [Credits](#Credits)
- [License](#License)

## Installation

Add a NuGet reference to the [Capgemini.PowerApps.Deployment](https://www.nuget.org/packages/Capgemini.PowerApps.Deployment) package in your Package Deployer package project. For more information on how to create the project, refer to Microsoft [documentation](https://docs.microsoft.com/en-us/powerapps/developer/common-data-service/package-deployer/create-packages-package-deployer#step-1-create-a-project-using-the-template)

Update your `PackageTemplate` file (created by following the Microsoft documentation) to inherit from `CapgeminiPackageTemplate` e.g.

```csharp
    public class PackageTemplate : CapgeminiPackageTemplate
    {
    }
```

## Usage

### Deactivate SLAs during import

Deploying SLAs to an instance where they are already activated can cause problems during solution import. The package template will automatically deactivate SLAs pre-deployment and activate them again post-deployment.

### Deactivate/activate processes

You can specify which processes to deactivate on import by adding a `processestodeactivate` element within the `configdatastorage` element of the `ImportConfig.xml` file.

```xml
<configdatastorage>
    <processestodeactivate>
        <processtodeactivate>The name of the process</processtodeactivate>
    </processestodeactivate>
</configdatastorage>
```

### Deactivate/activate plug-ins steps

You can specify which plug-in steps to deactivate on import by adding an `sdkstepstodeactivate` element within the `configdatastorage` element of the `ImportConfig.xml`.

```xml
<configdatastorage>
    <sdkstepstodeactivate>
        <sdksteptodeactivate name="The name of the SDK step" />
    </sdkstepstodeactivate>
</configdatastorage>
```

### Migrate data

You can migrate data using Capgemini's [data migrator tool](https://github.com/Capgemini/xrm-datamigration) by adding a `dataimports` element within the `configdatastorage` element of the `ImportConfig.xml`. There are three attributes that can be added to the individual `dataimport` elements.

- `datafolderpath` - this is the path to the folder that contains the data files extracted using the data migrator tool.
- `importconfigpath` - this is the path to the json file containing the import configuration for the data migrator tool
- `importbeforesolutions` - this allows you to specify whether the data should be imported before or after importing the solutions.

```xml
<configdatastorage>
    <dataimports>
        <dataimport 
            datafolderpath="ConfigurationData/Extract"
            importconfigpath="ConfigurationData/ImportConfig.json"
            importbeforesolutions="true"/>
    </dataimports>
</configdatastorage>
```

### Deploying word templates

You can import word templates by adding a `wordtemplates` element within the `configdatastorage` element of the `ImportConfig.xml`.

```xml
<configdatastorage>
    <wordtemplates>
        <wordtemplates name="Word Template.docx">
    </wordtemplates>
</configdatastorage>
```

### Setting SLAs as default

You can configure which SLAs should be set as default after import by adding a `defaultslas` within the `configdatastorage` element of the `ImportConfig.xml`.

```xml
<configdatastorage>
    <defaultslas>
        <defaultsla>The name of the SLA</defaultsla>
    </defaultslas>
</configdatastorage>
```

### Upgrade or update based on solution version

You can configure the template to either update or upgrade based on a semantic solution versioning scheme. This is done by adding the following attributes to the `configdatastorage` element. This may allow you to achieve faster deployment times if you only delete solution components on major version changes (for example).

```xml
<configdatastorage
    useupdateformajorversions="true"
    useupdateforminorversions="false"
    useupdateforpatchversions="false">
</configdatastorage>
```
## Contributing

To contribute to this project, report any bugs, submit new feature requests, submit changes via pull requests or even join in the overall design of the tool.

## Credits

Special thanks to the entire Capgemini community for their support in developing this tool.

## License

The Xrm Deployment and Xrm Deployment Package Deployer are released under the [MIT](LICENSE) license.
