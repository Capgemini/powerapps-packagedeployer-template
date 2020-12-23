# Contributing

Please first discuss the change you wish to make via an issue before making a change. 

## Pull request process

1. Ensure that there are automated tests that cover any changes 
1. Update the README.md with details of any significant changes to functionality
1. Ensure that your commit messages increment the version using [GitVersion syntax](https://gitversion.readthedocs.io/en/latest/input/docs/more-info/version-increments/). If no message is found then the patch version will be incremented by default.
1. You may merge the pull request once it meets all of the required checks. If you do not have permision, a reviewer will do it for you

## Lifecycle Call

Below is a summary of the different methods and when they are called during the lifecycle of the package deployer process. These details were taken from the microsoft docs found [here](https://docs.microsoft.com/en-us/power-platform/alm/package-deployer-tool#step-5-define-custom-code-for-your-package).

`InitializeCustomExtension` - Called to Initialize any functions in the Custom Extension.
`OverrideConfigurationDataFileLanguage` - Allows the currently selected language for data import to be overridden by a user selection.
Per solution:
    `OverrideSolutionImportDecision` - Called by the Solution Import subsystem after a decision is made by the import system.
    `PreSolutionImport` - Raised before the named solution is imported to allow for any configuration settings to be made to the import process.
    `RunSolutionUpgradeMigrationStep` - Is called during a solution upgrade when both solutions, old and Holding, are present in the system.
`BeforeImportStage` - Called before the Main Import process begins, after solutions and data.
`AfterPrimaryImport` - Called After all Import steps are complete, allowing for final customizations or tweaking of the CRM instance.