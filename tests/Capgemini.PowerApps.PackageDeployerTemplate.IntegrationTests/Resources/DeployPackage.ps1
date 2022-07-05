Write-Host "Installing PAC CLI..."

nuget install Microsoft.PowerApps.CLI -OutputDirectory pac

$pacNugetFolder = Get-ChildItem "pac" | Where-Object {$_.Name -match "Microsoft.PowerApps.CLI."}
$pacPath = $pacNugetFolder.FullName + "\tools"
$env:PATH = $env:PATH + ";" + $pacPath

$packageName = "Capgemini.PowerApps.PackageDeployerTemplate.MockPackage.dll"
$pacAuthName = "$(New-Guid)".Replace("-", "").SubString(0, 20)

Write-Host "Create Auth profile with name $pacAuthName..."
pac auth create --name $pacAuthName --url $env:CAPGEMINI_PACKAGE_DEPLOYER_TESTS_URL --username $env:CAPGEMINI_PACKAGE_DEPLOYER_TESTS_USERNAME --password $env:CAPGEMINI_PACKAGE_DEPLOYER_TESTS_PASSWORD 

try {
  Write-Host "Running Deploy command..."
  pac package deploy --package $packageName --logConsole
}
catch {
  Write-Host "An error occurred:"
  Write-Host $_.ScriptStackTrace
}
finally {
  Write-Host "Deleting Auth profile with name $pacAuthName..."
  pac auth delete --name $pacAuthName
}


exit 0