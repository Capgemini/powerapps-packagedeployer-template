$ErrorActionPreference = "Stop"

Install-Module -Name Microsoft.Xrm.Tooling.PackageDeployment.Powershell -Force

$connectionString = "Url=$env:CAPGEMINI_PACKAGE_DEPLOYER_TESTS_URL; Username=$env:CAPGEMINI_PACKAGE_DEPLOYER_TESTS_USERNAME; Password=$env:CAPGEMINI_PACKAGE_DEPLOYER_TESTS_PASSWORD; AuthType=OAuth; AppId=51f81489-12ee-4a9e-aaae-a2591f45987d; RedirectUri=app://58145B91-0C36-4500-8554-080854F2AC97"
$packageName = "Capgemini.PowerApps.PackageDeployerTemplate.MockPackage.dll"
$packageDirectory = Get-Location

Get-CrmPackages -PackageDirectory $packageDirectory -PackageName $packageName
Import-CrmPackage -CrmConnection $connectionString -PackageDirectory $packageDirectory -PackageName $packageName -LogWriteDirectory $packageDirectory -Verbose 