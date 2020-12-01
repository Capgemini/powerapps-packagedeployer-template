# Power Apps Package Deployer Template <img src="assets/Capgemini.PowerApps.PackageDeployerTemplate.svg" alt="Logo" height="32" />

[![Build Status](https://capgeminiuk.visualstudio.com/GitHub%20Support/_apis/build/status/CI-Builds/NuGet%20Packages/Capgemini.PowerAppsPackageDeployerTemplate?branchName=master)](https://capgeminiuk.visualstudio.com/GitHub%20Support/_build/latest?definitionId=205&branchName=master) [![Quality Gate Status](https://sonarcloud.io/api/project_badges/measure?project=Capgemini_xrm-packagedeployer&metric=alert_status)](https://sonarcloud.io/dashboard?id=Capgemini_xrm-packagedeployer) [![Coverage](https://sonarcloud.io/api/project_badges/measure?project=Capgemini_xrm-packagedeployer&metric=coverage)](https://sonarcloud.io/dashboard?id=Capgemini_xrm-packagedeployer)

A base template that introduces additional functionality when deploying Power Apps packages with the Package Deployer.

This project's aim is to build a powerful base Package Deployer template that simplifies common Power Apps package deployment tasks.

## Table of Contents

- [Installation](#Installation)
- [Usage](#Usage)
  - [Deactivate SLAs during import](#Deactivate-SLAs-during-import)
  - [Deactivate/activate processes](#Deactivateactivate-processes)
  - [Deactivate/activate plug-ins steps](#Deactivateactivate-plug-ins-steps)
  - [Migrate data](#Migrate-data)
  - [Deploying word templates](#Deploying-word-templates)
  - [Setting SLAs as default](#Setting-SLAs-as-default)
  - [Upgrade or update based on solution version](#Upgrade-or-update-based-on-solution-version)
- [Contributing](#Contributing)
- [Licence](#Licence)

## Installation

Add a NuGet reference to the [Capgemini.PowerApps.PackageDeployerTemplate](https://www.nuget.org/packages/Capgemini.PowerApps.PackageDeployerTemplate) package in your Package Deployer package project. For more information on how to create the project, refer to Microsoft [documentation](https://docs.microsoft.com/en-us/powerapps/developer/common-data-service/package-deployer/create-packages-package-deployer#step-1-create-a-project-using-the-template)

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

Please refer to the [Contributing](./CONTRIBUTING.md) guide.

## License

The Power Apps Package Deployer Template is released under the [MIT](./LICENSE) license.
