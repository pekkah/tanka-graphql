param (
    [string]$Output = "./artifacts"
 )

# Parameters
Write-Host "----------------------------------------"
Write-Host "Output: $Output"

# Git Information
$CurrentBranch = git branch --show-current | Out-String
$CurrentBranch = $CurrentBranch.Trim()
$Tag = git describe --tags --exact-match 2>$null | Out-String
$Tag = $Tag.Trim()

Write-Host "----------------------------------------"
Write-Host "CurrentBranch: $CurrentBranch"
Write-Host "Tag: $Tag"

if ($CurrentBranch -eq '' -and $Tag -eq '') {
    Write-Error "Not branch or tag"
    return
}

# Install tools
Write-Host "----------------------------------------"
Write-Host "Restoring dotnet tools"
dotnet tool restore

# Get GitVersion
Write-Host "----------------------------------------"
Write-Host "Getting GitVersion"
$Version = dotnet gitversion /output json /showvariable SemVer | Out-String
$Version = $Version.Trim()
Write-Host "Git version: $Version"
Write-Host "##vso[build.updatebuildnumber]$Version"

# Is Master Or Tag?
"----------------------------------------"
Write-Host "Check is master or tag"
$IsMasterOrTag = $CurrentBranch -eq 'master' -or $Tag -ne ''
Write-Host "Is tag or master: $IsMasterOrTag"


