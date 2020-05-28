$ErrorActionPreference = "Stop"

Set-Location "$PSScriptRoot\.."

robocopy /xc /xn /xo /s "." "../PackageDeployer"

Set-Location "../PackageDeployer"

Import-Module .\Microsoft.Xrm.Tooling.PackageDeployment.Powershell.dll -Force
Import-Module .\Microsoft.Xrm.Tooling.Connector.dll -Force

$connectionString = "Url=$env:CAPGEMINI_PACKAGE_DEPLOYER_TESTS_URL; Username=$env:CAPGEMINI_PACKAGE_DEPLOYER_TESTS_USERNAME; Password=$env:CAPGEMINI_PACKAGE_DEPLOYER_TESTS_PASSWORD; authtype=Office365"

$packageName = "Capgemini.PowerApps.Deployment.TestPackage.dll"
$packageDirectory = Get-Location

Get-CrmPackages -PackageDirectory $packageDirectory -PackageName $packageName
Import-CrmPackage -CrmConnection $connectionString -PackageDirectory $packageDirectory -PackageName $packageName -LogWriteDirectory $packageDirectory -Verbose 