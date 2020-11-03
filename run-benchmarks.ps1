param (
    [string]$CurrentBranch = $Env:BUILD_SOURCEBRANCH
 )

# Utils
function EnsureLastExitCode($message){
    if ($LASTEXITCODE -ne 0) {
        throw $message
    } 
}

# Git Information
if ($CurrentBranch -eq '') {
    $CurrentBranch = git branch --show-current | Out-String
    EnsureLastExitCode("git branch --show-current failed")
}

$CurrentBranch = $CurrentBranch.Trim()

"----------------------------------------"
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
"Benchmarks"
$BechmarkCmd = "dotnet run --project ./benchmarks/graphql.benchmarks/graphql.benchmarks.csproj -c Release --framework netcoreapp3.1 -- -i -m --filter *execution*"

if ($IsPreRelease) {
    $BechmarkCmd += " --job short"
}

Invoke-Expression $BechmarkCmd
EnsureLastExitCode("dotnet benchmarks failed")