[CmdletBinding]

# Install tools
dotnet tool restore

# Get Version
$Version = dotnet-gitversion /output json /showvariable SemVer | Out-String
Write-Host "##vso[build.updatebuildnumber]$Version"

# Is Master?
