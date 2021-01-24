# Contributing

Please first discuss the change you wish to make via an issue before making a change. 

## Pull request process

1. Ensure that there are automated tests that cover any changes 
1. Update the README.md with details of any significant changes to functionality
1. Ensure that your commit messages increment the version using [GitVersion syntax](https://gitversion.readthedocs.io/en/latest/input/docs/more-info/version-increments/). If no message is found then the patch version will be incremented by default.
1. You may merge the pull request once it meets all of the required checks. If you do not have permision, a reviewer will do it for you

## Lifecycle calls

Below is a summary of the different methods and when they are called during the lifecycle of the package deployer process. These details were taken from the microsoft docs found [here](https://docs.microsoft.com/en-us/power-platform/alm/package-deployer-tool#step-5-define-custom-code-for-your-package).

`InitializeCustomExtension` - Called to Initialize any functions in the Custom Extension.
`OverrideConfigurationDataFileLanguage` - Allows the currently selected language for data import to be overridden by a user selection.
Per solution:
    `OverrideSolutionImportDecision` - Called by the Solution Import subsystem after a decision is made by the import system.
    `PreSolutionImport` - Raised before the named solution is imported to allow for any configuration settings to be made to the import process.
    `RunSolutionUpgradeMigrationStep` - Is called during a solution upgrade when both solutions, old and Holding, are present in the system.
`BeforeImportStage` - Called before the Main Import process begins, after solutions and data.
`AfterPrimaryImport` - Called After all Import steps are complete, allowing for final customizations or tweaking of the CRM instance.

## Integration testing

A package template and solution have been added to this repository under the *tests* folder for the purposes of integration testing. This package and solution are automatically packed and deployed when running the integration tests.

In order to run the integration tests, you must configure a couple of environment variables:

| Environment variable                                       | Value                                                                                                                                                      |
|------------------------------------------------------------|------------------------------------------------------------------------------------------------------------------------------------------------------------|
| CAPGEMINI_PACKAGE_DEPLOYER_TESTS_URL                       | The URL of the Dataverse environment.                                                                                                                      |
| CAPGEMINI_PACKAGE_DEPLOYER_TESTS_USERNAME                  | The username to authenticate with when connecting to the Dataverse environment.                                                                            |
| CAPGEMINI_PACKAGE_DEPLOYER_TESTS_PASSWORD                  | The password to authenticate with when connecting to the Dataverse environment.                                                                            |
| PACKAGEDEPLOYER_SETTINGS_CONNREF_PDT_SHAREDAPPROVALS_D7DCB | The connection name of an Approvals connection. See [set connection references](./README.md#Set-connection-references) for how to get the connection name. |

In some cases, you may need to make changes to the solution to enable you to write integration tests. You first need to produced an unmanaged solution zip from the source by executing this command within the solution project folder:

```shell
dotnet build -p:SolutionPackageType=Unmanaged
```

Extracting this solution to source control after making your changes is not yet automated and so must be done manually. A shortcut is to delete the entire solution project folder and use the Power Apps CLI with the `pac solution clone` command in the *tests* folder.

```shell
pac solution clone -n pdt_PackageDeployerTemplate_MockSolution
```

After doing this, undo any changes to the solution project folder in Git *except* for changes under the *src* folder.