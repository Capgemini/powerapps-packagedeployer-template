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
    - [Set process states](#Set-process-states)
  - [SDK Steps](#SDK-stpes)
    - [Set SDK step states](#Set-sdk-step-states)
  - [Connection references](#Connection-references)
    - [Set connection references](#Set-connection-references)
  - [Environment variables](#Environment-variables)
    - [Set environment variables](#Set-environment-variables)
  - [Data](#Data)
    - [Import data](#Import-data)
  - [Word templates](#Word-templates)
    - [Import word templates](#Import-word-templates)
  - [Attribute specific functionality](#Attribute-specific-functionality)
    - [Set auto-number seed values](#Set-auto-number-seed-values)
  - [Mailboxes](#Mailboxes)
    - [Update, approve, test and enable shared mailboxes](#Update-approve-test-and-enable-shared-mailboxes)
- [Azure Pipelines](#Azure-pipelines)
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

All configuration for the package is placed within a `<templateconfig>` element in your package's ImportConfig.xml file.

### SLAs (classic)

**Note: this functionality is for the old SLA component - not the new SLA KPI component.**

#### Deactivate SLAs during import

Deploying SLAs to an instance where they are already activated can cause problems during solution import. The package template will automatically deactivate all SLAs pre-deployment and activate all SLAs post-deployment. If you want to disable this functionality, you can add an `activatedeactivateslas` attribute to the `<templateconfig>` element.

```xml
<templateconfig activatedeactivateslas="false">
</templateconfig>
```

#### Set SLAs as default

Set SLAs as default after the deployment by setting the `isdefault` attribute on an `<sla>` element.

```xml
<templateconfig>
    <slas>
        <sla name="The name of the SLA<" isdefault="true" />
    </slas>
</templateconfig>
```

### Processes

#### Set process states

All processes within the deployed solution(s) are activated by default after the deployment. You can manually set the state of processes after the deployment by setting the `state` attribute on a `<process>` element.

```xml
<templateconfig>
    <processes>
        <process name="The name of the process" state="Inactive" />
    </processes>
</templateconfig>
```

If your deployment is running as an application user then you may face [some issues](https://github.com/MicrosoftDocs/power-automate-docs/issues/216) if your solution contains flows. If you wish to continue deploying as an application user, you can pass the `LicensedUsername` runtime setting to the Package Deployer (or set the `PACKAGEDEPLOYER_SETTINGS_LICENSEDUSERNAME` environment variable) and this user will be impersonated for flow activation.

> You can also activate or deactivate processes that are not in your package by setting the `external` attribute to `true` on a `<process>` element. Be careful when doing this - deploying your package may introduce side-effects to an environment that make it incompatible with other solutions.

### SDK steps

#### Set SDK step states

All SDK steps within the deployed solution(s) are activated by default after the deployment. You can manually set the state of SDK steps after the deployment by setting the `state` attribute on an `<sdkstep>` element.

```xml
<templateconfig>
    <sdksteps>
        <sdkstep name="The name of the SDK step" state="Inactive" />
    </sdksteps>
</templateconfig>
```

> You can also activate or deactivate SDK steps that are not in your package by setting the `external` attribute to `true` on an `<sdkstep>` element. Be careful when doing this - deploying your package may introduce side-effects to an environment that make it incompatible with other solutions.

### Connection references

#### Set connection references

You can set connections for connection references either through environment variables (for example, those [exposed on Azure Pipelines](https://docs.microsoft.com/en-us/azure/devops/pipelines/process/variables?view=azure-devops&tabs=yaml%2Cbatch#access-variables-through-the-environment) from your variables or variable groups) or through Package Deployer [runtime settings](https://docs.microsoft.com/en-us/power-platform/admin/deploy-packages-using-package-deployer-windows-powershell#use-the-cmdlet-to-deploy-packages).

Environment variables must be prefixed with `PACKAGEDEPLOYER_SETTINGS_CONNREF_` and followed by the logical name. Similarly, runtime settings must be prefixed with `ConnRef:` and followed by the connection reference logical name. For example, if a connection reference logical name was `devhub_sharedvisualstudioteamservices_ca653`, this could be set via either of the following:

**Environment variable**

```powershell
$env:PACKAGEDEPLOYER_SETTINGS_CONNREF_DEVHUB_SHAREDVISUALSTUDIOTEAMSERVICES_CA653 = "shared-visualstudiot-44dd3131-3292-482a-9ec3-32cd7f3e799b"

Import-CrmPackage [...]
```

**Runtime setting**

```powershell
$runtimeSettings =  "ConnRef:devhub_sharedvisualstudioteamservices_ca653=shared-visualstudiot-44dd3131-3292-482a-9ec3-32cd7f3e799b"

Import-CrmPackage [...] –RuntimePackageSettings $runtimeSettings
```

The runtime setting takes precedence if both an environment variable and runtime setting are found for the same connection reference.

To get your flow connection names, go to your environment and navigate to _Data -> Connections_ within the [Maker Portal](https://make.powerapps.com). Opening a connection will reveal the connection name in the URL, which will have a format of 'environments/environmentid/connections/apiname/_connectionname_/details'. 

As above, you will need to pass a licensed user's email via runtime settings or environment variables if the Package Deployer is not running in the context of a licensed user. In addition, **the connections passed in need to be owned by the user doing the deployment or impersonated by the deployment**.

### Environment variables

#### Set environment variables

You can set Power App environment variables either through system environment variables (for example, those [exposed on Azure Pipelines](https://docs.microsoft.com/en-us/azure/devops/pipelines/process/variables?view=azure-devops&tabs=yaml%2Cbatch#access-variables-through-the-environment) from your variables or variable groups) or through Package Deployer [runtime settings](https://docs.microsoft.com/en-us/power-platform/admin/deploy-packages-using-package-deployer-windows-powershell#use-the-cmdlet-to-deploy-packages).

Environment variables must be prefixed with `PACKAGEDEPLOYER_SETTINGS_ENVVAR_` and followed by the schema name. Similarly, runtime settings must be prefixed with `EnvVar:` and followed by the environment variable schema name. For example, if an enviroment variable schema name was `pdt_testvariable`, this could be set via either of the following:

**Environment variable**

```powershell
$env:PACKAGEDEPLOYER_SETTINGS_ENVVAR_PDT_TESTVARIABLE = "test_value"

Import-CrmPackage [...]
```

**Runtime setting**

```powershell
$runtimeSettings = "EnvVar:pdt_testvariable=test_value"

Import-CrmPackage [...] –RuntimePackageSettings $runtimeSettings
```

The runtime setting takes precedence if both an environment variable and runtime setting are found for the same Power App environment variable.

If a value has already been set in the target environment then it will be overridden, otherwise a new environment variable value will be created and related to the environment variable definition determined by the given schema name.

### Data

#### Import data

You can migrate data using the Dataverse [data migrator tool](https://github.com/Capgemini/xrm-datamigration) by adding a `<dataimports>` element within the `<templateconfig>`. There are three attributes that can be added to the individual `<dataimport>` elements.

- `datafolderpath` - this is the path to the folder that contains the data files extracted using the data migrator tool.
- `importconfigpath` - this is the path to the json file containing the import configuration for the data migrator tool
- `importbeforesolutions` - this allows you to specify whether the data should be imported before or after importing the solutions.

```xml
<templateconfig>
    <dataimports>
        <dataimport 
            datafolderpath="ConfigurationData/Extract"
            importconfigpath="ConfigurationData/ImportConfig.json"
            importbeforesolutions="true"/>
    </dataimports>
</templateconfig>
```

### Word templates

#### Import word templates

You can import word templates by adding `<documenttemplate>` elements.

```xml
<templateconfig>
    <documenttemplates>
        <documenttemplate path="Word Template.docx">
    </documenttemplates>
</templateconfig>
```

### Attribute specific functionality

#### Set auto-number seed values

When deploying auto-numbers, seed values can be defined in the template for each entity. These are set post-deployment. Setting the `<autonumberseedvalue>` element determines that the column is an auto-number.

```xml
<templateconfig>
    <tables>
      <table name="account">
        <columns>
          <column name="new_accountautonumber" autonumberseedvalue="1"/>
        </columns>
      </table>
      <table name="contact">
        <columns>
          <column name="new_contactautonumber" autonumberseedvalue="2000"/>
        </columns>
      </table>
    </tables>
</templateconfig>
```
**Important Note**: When you set a seed value, it will reset the next number in the sequence to the seed value. Unless the autonumber column has an alternate key, it will not be enforced as unique. This means you could accidentally reset the count and end up with duplicate auto-number values.

You should set the seed once in the config file and avoid changing it. If you need to change the seed, ensure that it is a higher value than the current value on all target environments.

More information can be read on this functionality here: https://docs.microsoft.com/en-us/dynamics365/customerengagement/on-premises/developer/create-auto-number-attributes

### Mailboxes

#### Update, approve, test and enable shared mailboxes

You can update shared mailboxes with target email address, approve and test&enable by setting configurations either through environment variables (for example, those [exposed on Azure Pipelines](https://docs.microsoft.com/en-us/azure/devops/pipelines/process/variables?view=azure-devops&tabs=yaml%2Cbatch#access-variables-through-the-environment) from your variables or variable groups) or through Package Deployer [runtime settings](https://docs.microsoft.com/en-us/power-platform/admin/deploy-packages-using-package-deployer-windows-powershell#use-the-cmdlet-to-deploy-packages).

Environment variables must be prefixed with `PACKAGEDEPLOYER_SETTINGS_MAILBOX_` and followed by the source email address. Similarly, runtime settings must be prefixed with `Mailbox:` and followed by the source email address. For example, if a source email address was `support-dev@fake.com`, this could be set via either of the following:

**Environment variable**

```powershell
$env:PACKAGEDEPLOYER_SETTINGS_MAILBOX_SUPPORT-DEV@FAKE.COM = "support-prod@fake.com"

Import-CrmPackage [...]
```

**Runtime setting**

```powershell
$runtimeSettings = "Mailbox:support-dev@fake.com=support-prod@fake.com" 

Import-CrmPackage [...] –RuntimePackageSettings $runtimeSettings
```

The runtime setting takes precedence if both an environment variable and runtime setting are found for the same shared mailbox.


## Azure Pipelines

The template will automatically detect if your deployment is running on Azure Pipelines and will log with the appropriate [logging commands](https://docs.microsoft.com/en-us/azure/devops/pipelines/scripts/logging-commands) to ensure that warnings and errors are reported on your pipeline or release.

Use the `TraceLoggerAdapter` rather than the `PackageLog` to ensure that your extensions also integrate seamlessly with Azure Pipelines.
 
## Contributing

Please refer to the [Contributing](./CONTRIBUTING.md) guide.

## License

The Power Apps Package Deployer Template is released under the [MIT](./LICENSE) license.
