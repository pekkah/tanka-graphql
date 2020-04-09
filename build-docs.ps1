param (
    [string]$Output = "./artifacts/gh-pages",
    [string]$CurrentBranch = $Env:BUILD_SOURCEBRANCH
 )

# Utils
function EnsureLastExitCode($message){
    if ($LASTEXITCODE -ne 0) {
        throw $message
    } 
}

# Parameters
"----------------------------------------"
"Output: $Output"
$Location = Get-Location
"Location: $Location"

if ((Test-Path $output) -eq $True) {
    "Clean: $Output"
    Remove-Item -Recurse -Force $Output
}

# Git Information
"----------------------------------------"
if ($CurrentBranch -eq '') {
    $CurrentBranch = git branch --show-current | Out-String
    EnsureLastExitCode("git branch --show-current failed")
}

$CurrentBranch = $CurrentBranch.Trim()

"CurrentBranch: $CurrentBranch"

if ($CurrentBranch -eq '') {
    Write-Error "Not branch or tag"
    return
}

# Install tools
"----------------------------------------"
"Restoring dotnet tools"
dotnet tool restore

EnsureLastExitCode("Restore failed")

# Get GitVersion
"----------------------------------------"
"Getting GitVersion"
$Version = dotnet gitversion /output json /showvariable SemVer | Out-String -NoNewline
EnsureLastExitCode("Could not get SemVer from gitversion")
$Version = $Version.Trim()
"Git version: '$Version'"
"##vso[build.updatebuildnumber]$Version"

$PreReleaseTag = dotnet gitversion /output json /showvariable PreReleaseTag | Out-String -NoNewline
EnsureLastExitCode("Could not get PrReleaseTag from gitversion")
$PreReleaseTag = $PreReleaseTag.Trim()
$IsPreRelease = $PreReleaseTag -ne ''
"PreReleaseTag: $PreReleaseTag, IsPreRelease: $IsPreRelease"

"----------------------------------------"
"Docs"
$DocsOutput = $Output
$Basepath = "/tanka-graphql/"

if ($IsPreRelease) {
    $DocsOutput += "/beta"
    $Basepath += "beta/"
}

"Output: $DocsOutput"
"BasePath: $Basepath"

dotnet tanka-docs --output $DocsOutput --basepath $Basepath
EnsureLastExitCode("dotnet tanka-docs failed")

"----------------------------------------"
"DONE"
Set-Location $Location
