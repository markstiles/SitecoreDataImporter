<#
    This script creates a local.properties file for nant to build from
#>

# Constants
$localProp = "local.properties"
$localPropExample = "local.properties.example"

# Main
try 
{
    $actualPath = "$PSScriptRoot\properties\$localProp"
    if (Test-Path $actualPath)
    {
	Write-Host "$actualPath already exists"
        return
    }
	
    $examplePath = "$PSScriptRoot\properties\$localPropExample"
    if (-not (Test-Path $examplePath))
    {
        throw [System.IO.DirectoryNotFoundException] "$examplePath not found."
    }

    Copy-Item -Path "$examplePath" -Destination "$actualPath" -Force
}
catch [System.Exception]
{
    Write-Host $error[0]
    exit
}