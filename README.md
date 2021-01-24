# Power Apps Package Deployer Template <img src="src\Capgemini.PowerApps.PackageDeployerTemplate\images\Capgemini.PowerApps.PackageDeployerTemplate.svg" alt="Logo" height="32" />

[![Build Status](https://capgeminiuk.visualstudio.com/GitHub%20Support/_apis/build/status/CI-Builds/NuGet%20Packages/Capgemini.PowerAppsPackageDeployerTemplate?branchName=master)](https://capgeminiuk.visualstudio.com/GitHub%20Support/_build/latest?definitionId=205&branchName=master) [![Quality Gate Status](https://sonarcloud.io/api/project_badges/measure?project=Capgemini_xrm-packagedeployer&metric=alert_status)](https://sonarcloud.io/dashboard?id=Capgemini_xrm-packagedeployer) [![Coverage](https://sonarcloud.io/api/project_badges/measure?project=Capgemini_xrm-packagedeployer&metric=coverage)](https://sonarcloud.io/dashboard?id=Capgemini_xrm-packagedeployer)

A base template that introduces additional functionality when deploying Power Apps packages with the Package Deployer.

This project's aim is to build a powerful base Package Deployer template that simplifies common Power Apps package deployment tasks.

## Table of contents

- [Installation](#Installation)
- [Usage](#Usage)
  - [SLAs (classic)](#SLAs-classic)
    - [Deactivate SLAs during import](#Deactivate-SLAs-during-import)
    - [Set SLAs as default](#Set-SLAs-as-default)
  - [Processes](#Processes)
    - [Deactivate processes](#Deactivate-processes)
    - [Activate processes](#Activate-processes)
  - [SDK Steps](#SDK-stpes)
    - [Deactivate SDK steps](#Deactivate-SDK-steps)
  - [Flows](#Flows)
    - [Deactivate flows](#Deactivate-flows)
    - [Set connection references](#Set-connection-references)
  - [Data](#Data)
    - [Import data](#Import-data)
  - [Word templates](#Word-templates)
    - [Import word templates](#Import-word-templates)
- [Contributing](#Contributing)
- [Licence](#Licence)

## Installation

Add a NuGet reference to the [Capgemini.PowerApps.PackageDeployerTemplate](https://www.nuget.org/packages/Capgemini.PowerApps.PackageDeployerTemplate) package in your Package Deployer package project. For more information on how to create the project, refer to Microsoft [documentation](https://docs.microsoft.com/en-us/powerapps/developer/common-data-service/package-deployer/create-packages-package-deployer#step-1-create-a-project-using-the-template)

Update your `PackageTemplate` file (created by following the Microsoft documentation) to inherit from `PackageTemplateBase` e.g.

```csharp
    public class PackageTemplate : PackageTemplateBase
    {
    }
```

## Usage

### SLAs (classic)

**Note: this functionality is for the old SLA component - not the new SLA KPI component.**

#### Deactivate SLAs during import

Deploying SLAs to an instance where they are already activated can cause problems during solution import. The package template will automatically deactivate SLAs pre-deployment and activate them again post-deployment. If you want to disable this functionality, you can add an `activatedeactivateslas` attribute to the `configdatastorage` element.

```xml
<configdatastorage activatedeactivateslas="false">
</configdatastorage>
```

#### Set SLAs as default

You can configure which SLAs should be set as default after import by adding a `defaultslas` within the `configdatastorage` element of the `ImportConfig.xml`.

```xml
<configdatastorage>
    <defaultslas>
        <defaultsla>The name of the SLA</defaultsla>
    </defaultslas>
</configdatastorage>
```

### Processes

#### Deactivate processes

You can specify which processes to deactivate post-import by adding a `processestodeactivate` element within the `configdatastorage` element of the `ImportConfig.xml` file. This executes before data is imported (unless the data is explictly imported before solutions).

```xml
<configdatastorage>
    <processestodeactivate>
        <processtodeactivate>The name of the process</processtodeactivate>
    </processestodeactivate>
</configdatastorage>
```

#### Activate processes

You can specify which processes to activate after your post-solution data is imported by adding a `processestoactivate` element within the `configdatastorage` element of the `ImportConfig.xml` file. 

This can be useful where you want to activate processes that are dependent on data that imports after your solutions (e.g. any data for custom entities. Another example might be where you use this in conjunction with `processestodeactivate` for any worklows that you don't want active during the data import but that you do want active after the package has been deployed.

```xml
<configdatastorage>
    <processestoactivate>
        <processtoactivate>The name of the process</processtoactivate>
    </processestoactivate>
</configdatastorage>
```

### SDK steps

#### Deactivate SDK steps

You can specify which plug-in steps to deactivate on import by adding an `sdkstepstodeactivate` element within the `configdatastorage` element of the `ImportConfig.xml`. This executes before data is imported (unless the data is explictly imported before solutions).

```xml
<configdatastorage>
    <sdkstepstodeactivate>
        <sdksteptodeactivate name="The name of the SDK step" />
    </sdkstepstodeactivate>
</configdatastorage>
```
### Flows

#### Deactivate flows

You can configure which flows should be disabled after import by adding a `flowstodeactivate` element within the `configdatastorage` element of the `ImportConfig.xml`. Any flows not listed here will be enabled by default. 

```xml
<configdatastorage>
    <flowstodeactivate>
        <flowtodeactivate>Name of the flow to deactivate</flowtodeactivate>
    </flowstodeactivate>
</configdatastorage>
```

If your deployment is running as an application user then you may face [some issues](https://github.com/MicrosoftDocs/power-automate-docs/issues/216). If you wish to continue deploying as an application user, you can pass the `LicensedUsername` and `LicensedPassword` runtime settings to the Package Deployer (or set the `PACKAGEDEPLOYER_SETTINGS_LICENSEDUSERNAME and `PACKAGEDEPLOYER_SETTINGS_LICENSEDPASSWORD` environment variables) and these credentials will be used for interacting with flows.

#### Set connection references

You can set connections for connection references either through environment variables (for example, those [exposed on Azure Pipelines](https://docs.microsoft.com/en-us/azure/devops/pipelines/process/variables?view=azure-devops&tabs=yaml%2Cbatch#access-variables-through-the-environment) from your variables or variable groups) or through Package Deployer [runtime settings](https://docs.microsoft.com/en-us/power-platform/admin/deploy-packages-using-package-deployer-windows-powershell#use-the-cmdlet-to-deploy-packages).

Environment variables must be prefixed with `PACKAGEDEPLOYER_SETTINGS_CONNREF_` and followed by the logical name. Similarly, runtime settings must be prefixed with `ConnRef:` and followed by the connection reference logical name. For example, if a connection reference logical name was `devhub_sharedvisualstudioteamservices_ca653`, this could be set via either of the following:

**Environment variable**

```powershell
$env:PACKAGEDEPLOYER_SETTINGS_CONNREF_DEVHUB_SHAREDVISUALSTUDIOTEAMSERVICES_CA653 = "shared-visualstudiot-44dd3131-3292-482a-9ec3-32cd7f3e799b"
```

**Runtime setting**

```powershell
$runtimeSettings = @{ 
    "ConnRef:devhub_sharedvisualstudioteamservices_ca653" = "shared-visualstudiot-44dd3131-3292-482a-9ec3-32cd7f3e799b" 
}

Import-CrmPackage –CrmConnection $conn –PackageDirectory $packageDir –PackageName Package.dll –RuntimePackageSettings $runtimeSettings
```

The runtime setting takes precedence if both an environment variable and runtime setting are found for the same connection reference.

To get your flow connection names, go to your environment and navigate to _Data -> Connections_ within the [Maker Portal](https://make.powerapps.com). Opening a connection will reveal the connection name in the URL, which will have a format of 'environments/environmentid/connections/apiname/_connectionname_/details'. 

As above, you will need to pass licensed user credentials via runtime settings or environment variables if the Package Deployer is not running in the context of a licensed user. In addition, **the connections passed in need to be owned by the user doing the deployment**.

### Data

#### Import data

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

### Word templates

#### Import word templates

You can import word templates by adding a `wordtemplates` element within the `configdatastorage` element of the `ImportConfig.xml`.

```xml
<configdatastorage>
    <wordtemplates>
        <wordtemplates name="Word Template.docx">
    </wordtemplates>
</configdatastorage>
```

## Contributing

Please refer to the [Contributing](./CONTRIBUTING.md) guide.

## License

The Power Apps Package Deployer Template is released under the [MIT](./LICENSE) license.
