$ErrorActionPreference = "Stop"

Set-Location "$PSScriptRoot\.."

Install-Module -Name Microsoft.Xrm.Tooling.PackageDeployment.Powershell -Force

$connectionString = "Url=$env:CAPGEMINI_PACKAGE_DEPLOYER_TESTS_URL; Username=$env:CAPGEMINI_PACKAGE_DEPLOYER_TESTS_USERNAME; Password=$env:CAPGEMINI_PACKAGE_DEPLOYER_TESTS_PASSWORD; authtype=Office365"

$packageName = "Capgemini.PowerApps.PackageDeployerTemplate.TestPackage.dll"
$packageDirectory = Get-Location

Get-CrmPackages -PackageDirectory $packageDirectory -PackageName $packageName
Import-CrmPackage -CrmConnection $connectionString -PackageDirectory $packageDirectory -PackageName $packageName -LogWriteDirectory $packageDirectory -Verbose 