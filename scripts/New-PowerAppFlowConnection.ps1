#
# New-PowerAppFlowConnection.ps1
#
[CmdletBinding()]
param (
    [string]
    [Parameter(Mandatory = $true)]
    $Username,
    [securestring]
    [Parameter(Mandatory = $true)] 
    $Password,
    [Parameter(Mandatory = $true)]
    [string]
    $EnvironmentName,
    [Parameter(Mandatory = $true)]
    [string]
    $Region,
    [Parameter(Mandatory = $true)]
    [string]
    $Connector,
    [Parameter(Mandatory = $false)]
    [hashtable]
    $ConnectionParameters = @{},
    [string]
    [parameter(Mandatory = $false)]
    $DisplayName,
    [string]
    [Parameter(Mandatory = $false)]
    $OutputVariable
)

$ErrorActionPreference = 'Stop'

Install-Module -Name Microsoft.PowerApps.Administration.PowerShell -Force -Scope CurrentUser -AllowClobber

Write-Host "Authenticating as $Username."
Add-PowerAppsAccount -Username $Username -Password $Password

if (!$ConnectionId) {
    $ConnectionId = [Guid]::NewGuid().ToString("N")    
}

$body = @{ 
    properties = @{ 
        environment          = @{ 
            name = "$EnvironmentName" 
        } 
        connectionParameters = $ConnectionParameters
        displayName          = $DisplayName
    } 
}

Write-Host "Creating $Connector connection with ID $ConnectionId on environment $EnvironmentName with $($ConnectionParameters.connectionString)"

$result = InvokeApi `
    -Method PUT `
    -Route "https://$Region.api.powerapps.com/providers/Microsoft.PowerApps/apis/$Connector/connections/$($ConnectionId)?api-version=2021-02-01&`$filter=environment eq '$EnvironmentName'" `
    -Body $body `
    -ThrowOnFailure

Write-Host "Connection $($result.name) created."

if ($OutputVariable) {
    Write-Host "Setting $OutputVariable variable to $($result.name)."
    Write-Host "##vso[task.setvariable variable=$OutputVariable]$($result.name)"
}